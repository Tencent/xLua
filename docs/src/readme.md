---
title: 可以使用的markdown
type: detail
order: 0
---

## 主标题

title 为标题
type 固定为guide宝宝埋的坑
order 为左侧导航排序，但是注意，假设0开始是基础的区域，100后是教程的区域，如果你想把文章放到基础区域，那么order必须小于100

> 这是小提示框用法

![图片用法](https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/xLua.png)

ps：markdown的图片不会调整大小所以如果要使用调整大小的图片应该用：

<img width="173" height="57" src="https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/xLua.png">


### 子标题会被索引

正常的文本内容哈哈哈哈哈
`这是高光内容`哈哈哈哈

### 子标题2

[超链接用法](https://github.com/tencent/xlua)

- 列表用法
- 列表用法
- 列表用法
- 列表用法
- 列表用法

**加粗用法**

<p class="tip">大提示框用法</p>

1. 有序列表用法
2. 有序列表用法
3. 有序列表用法

下面是分割线用法
***

``` csharp
这是代码块，支持的代码格式有：csharp ，lua，html, js, bash, css
```

下面是列表使用：

| Helloworld       | Helloworld   |
| ---------------- |:------------:|
| `Helloworld`     | Helloworld   |
| `Helloworld`     | Helloworld   |