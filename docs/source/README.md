# XLua 教程文档

这是XLua的教程文档网站，文档是使用[hexo](http://hexo.io/)构建的。 文档内容在[src](src)文件夹中，使用Markdown格式编写。

## 搭建本地文档环境

* 首先您必须安装[nodejs](http://nodejs.cn/)及[npm](https://www.npmjs.com/)

* 安装hexo，在docs/source下执行

```
$ npm install -g hexo-cli
$ npm install
```

* 如下指令在 `localhost:4000` 启动文档网站：

```
$ hexo server
```


## 修改Markdown后生成静态网页

* 已经安装了hexo的话，执行如下指令：

```
$ hexo g
```

