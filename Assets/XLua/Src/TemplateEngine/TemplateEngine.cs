/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using XLua;

namespace TemplateEngine
{
    public enum TokenType
    {
        Code, Eval, Text
    }

    public class Chunk
    {
        public TokenType Type {get; private set;}
        public string Text { get; private set; }
        public Chunk(TokenType type, string text)
        {
            Type = type;
            Text = text;
        }
    }

    class TemplateFormatException : Exception
    {
        public TemplateFormatException(string message)
        {
        }
    }

    public class Parser
    {
        public static string RegexString
        {
            get;
            private set;
        }

        static Parser()
        {
            RegexString = GetRegexString();
        }

        /// <summary>
        /// Replaces special characters with their literal representation.
        /// </summary>
        /// <returns>Resulting string.</returns>
        /// <param name="input">Input string.</param>
        static string EscapeString(string input)
        {
            var output = input
                .Replace("\\", @"\\")
                    .Replace("\'", @"\'")
                    .Replace("\"", @"\""")
                    .Replace("\n", @"\n")
                    .Replace("\t", @"\t")
                    .Replace("\r", @"\r")
                    .Replace("\b", @"\b")
                    .Replace("\f", @"\f")
                    .Replace("\a", @"\a")
                    .Replace("\v", @"\v")
                    .Replace("\0", @"\0");
            /*          var surrogateMin = (char)0xD800;
            var surrogateMax = (char)0xDFFF;
            for (char sur = surrogateMin; sur <= surrogateMax; sur++)
                output.Replace(sur, '\uFFFD');*/
            return output;
        }

        static string GetRegexString()
        {
            string regexBadUnopened = @"(?<error>((?!<%).)*%>)";
            string regexText = @"(?<text>((?!<%).)+)";
            string regexNoCode = @"(?<nocode><%=?%>)";
            string regexCode = @"<%(?<code>[^=]((?!<%|%>).)*)%>";
            string regexEval = @"<%=(?<eval>((?!<%|%>).)*)%>";
            string regexBadUnclosed = @"(?<error><%.*)";
            string regexBadEmpty = @"(?<error>^$)";

            return '(' + regexBadUnopened
                + '|' + regexText
                + '|' + regexNoCode
                + '|' + regexCode
                + '|' + regexEval
                + '|' + regexBadUnclosed
                + '|' + regexBadEmpty
                + ")*";
        }

        /// <summary>
        /// Parses the string into regex groups, 
        /// stores group:value pairs in List of Chunk
        /// <returns>List of group:value pairs.</returns>;
        /// </summary>
        public static List<Chunk> Parse(string snippet)
        {
            Regex templateRegex = new Regex(
                RegexString,
                RegexOptions.ExplicitCapture | RegexOptions.Singleline
            );
            Match matches = templateRegex.Match(snippet);

            if (matches.Groups["error"].Length > 0)
            {
                throw new TemplateFormatException("Messed up brackets");
            }

            List<Chunk> Chunks = matches.Groups["code"].Captures
                .Cast<Capture>()
                .Select(p => new { Type = TokenType.Code, p.Value, p.Index })
                .Concat(matches.Groups["text"].Captures
                .Cast<Capture>()
                .Select(p => new { Type = TokenType.Text, Value = EscapeString(p.Value), p.Index }))
                .Concat(matches.Groups["eval"].Captures
                .Cast<Capture>()
                .Select(p => new { Type = TokenType.Eval, p.Value, p.Index }))
                .OrderBy(p => p.Index)
                .Select(m => new Chunk(m.Type, m.Value))
                .ToList();

            if (Chunks.Count == 0)
            {
                throw new TemplateFormatException("Empty template");
            }
            return Chunks;
        }
    }

    public class LuaTemplate
    {
        public static string ComposeCode(List<Chunk> chunks)
        {
            StringBuilder code = new StringBuilder();

            code.Append("local __text_gen = ''\r\n");
            foreach (var chunk in chunks)
            {
                switch (chunk.Type)
                {
                    case TokenType.Text:
                        code.Append("__text_gen = __text_gen .. \"" + chunk.Text + "\"\r\n");
                        break;
                    case TokenType.Eval:
                        code.Append("__text_gen = __text_gen .. (" + chunk.Text + ")\r\n");
                        break;
                    case TokenType.Code:
                        code.Append(chunk.Text + "\r\n");
                        break;
                }
            }

            code.Append("return __text_gen\r\n");
            //UnityEngine.Debug.Log("code compose:"+code.ToString());
            return code.ToString();
        }

        public static LuaFunction Compile(LuaEnv luaenv, string snippet)
        {
            return luaenv.LoadString(ComposeCode(Parser.Parse(snippet)), "luatemplate");
        }

        public static string Execute(LuaFunction compiledTemplate, LuaTable parameters)
        {
            compiledTemplate.SetEnv(parameters);
            object[] result = compiledTemplate.Call();
            System.Diagnostics.Debug.Assert(result.Length == 1);
            return result[0].ToString();
        }

        public static string Execute(LuaFunction compiledTemplate)
        {
            object[] result = compiledTemplate.Call();
            System.Diagnostics.Debug.Assert(result.Length == 1);
            return result[0].ToString();
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int Compile(RealStatePtr L)
        {
			string snippet = LuaAPI.lua_tostring(L, 1);
            
            string code;
            try
            {
                code = ComposeCode(Parser.Parse(snippet));
            }
            catch (Exception e)
            {
				return LuaAPI.luaL_error(L, String.Format("template compile error:{0}\r\n", e.Message));
            }
            //UnityEngine.Debug.Log("code=" + code);
            if (LuaAPI.luaL_loadbuffer(L, code, "luatemplate") != 0)
            {
                return LuaAPI.lua_error(L);
            }
            return 1;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        public static int Execute(RealStatePtr L)
        {
            if (!LuaAPI.lua_isfunction(L, 1))
            {
                return LuaAPI.luaL_error(L, "invalid compiled template, function needed!\r\n");
            }
            if (LuaAPI.lua_istable(L, 2))
            {
                LuaAPI.lua_setfenv(L, 1);
            }
            LuaAPI.lua_pcall(L, 0, 1, 0);
            return 1;
        }

        static LuaCSFunction templateCompileFunction = Compile;
        static LuaCSFunction templateExecuteFunction = Execute;

        public static void OpenLib(RealStatePtr L)
        {
            LuaAPI.lua_newtable(L);
            LuaAPI.xlua_pushasciistring(L, "compile");
            LuaAPI.lua_pushstdcallcfunction(L, templateCompileFunction);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.xlua_pushasciistring(L, "execute");
            LuaAPI.lua_pushstdcallcfunction(L, templateExecuteFunction);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_setglobal(L, "template");
        }
    }
}





