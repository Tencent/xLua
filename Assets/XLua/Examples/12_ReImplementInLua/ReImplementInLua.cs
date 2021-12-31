using UnityEngine;
using System.Collections;
using XLua;

namespace XLuaTest
{

    [GCOptimize(OptimizeFlag.PackAsTable)]
    public struct PushAsTableStruct
    {
        public int x;
        public int y;
    }

    public class ReImplementInLua : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            LuaEnv luaenv = new LuaEnv();
            //这两个例子都必须生成代码才能正常运行
            //例子1：改造Vector3
            //沿用Vector3原来的映射方案Vector3 -> userdata，但是把Vector3的方法实现改为lua实现，通过xlua.genaccessor实现不经过C#直接操作内存
            //改为不经过C#的好处是性能更高，而且你可以省掉相应的生成代码以达成省text段的效果
            //仍然沿用映射方案的好处是userdata比table更省内存，但操作字段比table性能稍低，当然，你也可以结合例子2的思路，把Vector3也改为映射到table
            luaenv.DoString(@"
            function test_vector3(title, v1, v2)
               print(title)
               v1.x = 100
               print(v1.x, v1.y, v1.z)
               print(v1, v2)
               print(v1 + v2)
               v1:Set(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z)
               print(v1)
               print(CS.UnityEngine.Vector3.Normalize(v1))
            end
            test_vector3('----before change metatable----', CS.UnityEngine.Vector3(1, 2, 3), CS.UnityEngine.Vector3(7, 8, 9))

            local get_x, set_x = xlua.genaccessor(0, 8)
            local get_y, set_y = xlua.genaccessor(4, 8)
            local get_z, set_z = xlua.genaccessor(8, 8)
            
            local fields_getters = {
                x = get_x, y = get_y, z = get_z
            }
            local fields_setters = {
                x = set_x, y = set_y, z = set_z
            }

            local ins_methods = {
                Set = function(o, x, y, z)
                    set_x(o, x)
                    set_y(o, y)
                    set_z(o, z)
                end
            }

            local mt = {
                __index = function(o, k)
                    --print('__index', k)
                    if ins_methods[k] then return ins_methods[k] end
                    return fields_getters[k] and fields_getters[k](o)
                end,

                __newindex = function(o, k, v)
                    if fields_setters[k] then fields_setters[k](o, v) else error('no such field ' .. k) end
                end,

                __tostring = function(o)
                    return string.format('vector3 { %f, %f, %f}', o.x, o.y, o.z)
                end,

                __add = function(a, b)
                    return CS.UnityEngine.Vector3(a.x + b.x, a.y + b.y, a.z + b.z)
                end
            }

            xlua.setmetatable(CS.UnityEngine.Vector3, mt)
            test_vector3('----after change metatable----', CS.UnityEngine.Vector3(1, 2, 3), CS.UnityEngine.Vector3(7, 8, 9))
        ");

            Debug.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

            //例子2：struct映射到table改造
            //PushAsTableStruct传送到lua侧将会是table，例子里头还为这个table添加了一个成员方法SwapXY，静态方法Print，打印格式化，以及构造函数
            luaenv.DoString(@"
            local mt = {
                __index = {
                    SwapXY = function(o) --成员函数
                        o.x, o.y = o.y, o.x
                    end
                },

                __tostring = function(o) --打印格式化函数
                    return string.format('struct { %d, %d}', o.x, o.y)
                end,
            }

            xlua.setmetatable(CS.XLuaTest.PushAsTableStruct, mt)
            
            local PushAsTableStruct = {
                Print = function(o) --静态函数
                    print(o.x, o.y)
                end
            }

            setmetatable(PushAsTableStruct, {
                __call = function(_, x, y) --构造函数
                    return setmetatable({x = x, y = y}, mt)
                end
            })
            
            xlua.setclass(CS.XLuaTest, 'PushAsTableStruct', PushAsTableStruct)
        ");

            PushAsTableStruct test;
            test.x = 100;
            test.y = 200;
            luaenv.Global.Set("from_cs", test);

            luaenv.DoString(@"
            print('--------------from csharp---------------------')
            assert(type(from_cs) == 'table')
            print(from_cs)
            CS.XLuaTest.PushAsTableStruct.Print(from_cs)
            from_cs:SwapXY()
            print(from_cs)

            print('--------------from lua---------------------')
            local from_lua = CS.XLuaTest.PushAsTableStruct(4, 5)
            assert(type(from_lua) == 'table')
            print(from_lua)
            CS.XLuaTest.PushAsTableStruct.Print(from_lua)
            from_lua:SwapXY()
            print(from_lua)
        ");

            luaenv.Dispose();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
