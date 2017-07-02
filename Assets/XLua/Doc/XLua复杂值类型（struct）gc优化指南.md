## 复杂值类型的gc问题

xLua复杂值类型（struct）的默认传递方式是引用传递，这种方式要求先对值类型boxing，传递给lua，lua使用后释放该引用。由于值类型每次boxing将产生一个新对象，当lua侧使用完毕释放该对象的引用时，则产生一次gc。
为此，xLua实现了一套struct的gc优化方案，您只要通过简单的配置，则可以实现满足条件的struct传递到lua侧无gc。

## struct需要满足什么条件？

1. struct允许嵌套其它struct，但它以及它嵌套的struct只能包含这几种基本类型：byte、sbyte、short、ushort、int、uint、long、ulong、float、double；例如UnityEngine定义的大多数值类型：Vector系列，Quaternion，Color。。。均满足条件，或者用户自定义的一些struct
2. 该struct配置了GCOptimize属性（对于常用的UnityEngine的几个struct，Vector系列，Quaternion，Color。。。均已经配置了该属性），这个属性可以通过配置文件或者C# Attribute实现；
3. 使用到该struct的地方，需要添加到生成代码列表；

## 如何配置？

见`XLua的配置.md`的GCOptimize章节。