using UnityEngine;
using UnityEngine.UI;
using XLua;

[Hotfix]
public class HotfixTest : MonoBehaviour
{
    public Button btn_Hotfix;

    LuaEnv luaenv = new LuaEnv();
    public int tick = 0; //如果是private的，在lua设置xlua.private_accessible(CS.HotfixTest)后即可访问

    void Start()
    {
        // 使用Hotfix功能需在指定平台添加宏定义"HOTFIX_ENABLE"，使用前请仔细阅读相关说明文档
        btn_Hotfix.onClick.AddListener(Hotfix);
    }

    void Update()
    {
        if (++tick % 50 == 0)
        {
            Debug.Log(">>>>>>>>Update in C#, tick = " + tick);
        }
    }

    void Hotfix()
    {
        luaenv.DoString(@"
            xlua.hotfix(CS.HotfixTest, 'Update', function(self)
                self.tick = self.tick + 1
                if (self.tick % 50) == 0 then
                    print('<<<<<<<<Update in lua, tick = ' .. self.tick)
                end
            end)
        ");
    }
}
