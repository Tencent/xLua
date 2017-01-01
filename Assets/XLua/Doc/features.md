# 特性

## 总体

* Lua虚拟机支持
 * Lua5.3.3
 * Luajit2.1beta2
* Unity3D版本支持
 * Unity5
 * Unity4
* 平台支持
 * windows 64/32
 * android
 * ios 64/32/bitcode
 * osx
* 互访技术
 * 生成适配代码
 * 反射
* 易用性
 * 解压即可用
 * 开发期无需生成代码
 * 生成代码和反射间可无缝切换
 * 更简单的无GC api
 * 菜单简单易懂
 * 配置可以多份，按模块划分，也可以直接在目标类型上打Attribute标签
 * 自动生成link.xml防止代码剪裁
 * Plugins部分采用cmake编译，更简单
 * 核心代码不依赖生成代码，可以随时删除生成目录
* 性能
 * Lazyload技术，避免用不上的类型的开销
 * lua函数映射到c# delegate，lua table映射到interface，可实现接口层面无C# gc alloc开销
 * 所有基本值类型，所有枚举，字段都是值类型的struct，在Lua和C#间传递无C# gc alloc
 * LuaTable，LuaFunction提供无gc访问接口
 * 通过代码生成期的静态分析，生成最优代码
 * 支持C#和Lua间指针传递
 * 自动解除已经Destroy的UnityEngine.Object的引用
* 扩展性
 * 不用改代码就可以加入Lua第三方扩展
 * 生成引擎提供接口做二次开发
 
## 支持为如下C#实现打补丁

 * 构造函数
 * 析构函数
 * 成员函数
 * 静态函数
 * 泛化函数
 * 操作符重载
 * 成员属性
 * 静态属性
 * 事件
 
## Lua代码加载

* 加载字符串
 * 支持加载后立即执行
 * 支持加载后返回一个delegate或者LuaFunction，调用delegate或者LuaFunction后可传脚本参数
* Resources目录的文件
 * 直接require
* 自定义loader
 * Lua里头require时触发
 * require参数透传给loader，loader读取Lua代码返回
* Lua原有的方式
 * Lua原有的方式都保留
 
## Lua调用C#

* 创建C#对象
* C#静态属性，字段
* C#静态方法
* C#成员属性，字段
* C#成员方法
* C#继承
 * 子类对象可以直接调用父类的方法，访问父类属性
 * 子类模块可以直接调用父类的静态方法，静态属性
* 扩展方法（Extension methods）
 * 就像普通成员方法一样使用
* 参数的输入输出属性（out，ref）
 * out对应一个lua返回值
 * ref对应一个lua参数以及一个lua返回值
* 函数重载
 * 支持重载
 * 由于lua数据类型远比C#要少，会出现无法判断的情况，可通过扩展方法来来调用。
* 操作符重载
 * 支持的操作符：+，-，*，/，==，一元-，<，<=， %，[]
 * 其它操作符可以借助扩展方法调用
* 参数默认值
 * C#参数有默认值，在lua可以不传
* 可变参数
 * 在对应可变参数部分，直接输入一个个参数即可，不需要把这些参数扩到一个数组里头
* 泛化方法调用
 * 静态方法可以自行封装使用
 * 成员函数可通过扩展方法封装使用
* 枚举类型
 * 数字或字符串到枚举的转换
* delegate
 * 调用一个C# delegate
 * +操作符
 * -操作符
 * 把一个lua函数作为一个c# delegate传递给c#
* event
 * 增加事件回调
 * 移除事件回调
* 64位整数
 * 传递无gc而且无精度损失
 * lua53下使用原生64位支持
 * 可以和number运算
 * 以java的方式支持无符号64位整数
* table的自动转换到C#复杂类型
 * obj.complexField = {a = 1, b = {c = 1}}，obj是一个C#对象，complexField是两层嵌套的struct或者class
* typeof
 * 对应C#的typeof操作符，返回Type对象
* lua侧直接clone
* decimal
 * 传递无gc而且无精度损失

## C#调用Lua

* 调用Lua函数
 * 以delegate方式调用Lua函数
 * 以LuaFunction调用lua函数
* 访问Lua的table
 * LuaTable的泛化Get/Set接口，调用无gc，可指明Key，Value的类型
 * 用标注了CSharpCallLua的interface访问
 * 值拷贝到struct，class
 
## Lua虚拟机

* 虚拟机gc参数读取及设置

## 工具链

* Lua Profiler
 * 可根据函数调用总时长，平均每次调用时长，调用次数排序
 * 显示lua函数名及其所在文件的名字及行号
 * 如果C#函数，会显示这个是C#函数
* 支持真机调试

