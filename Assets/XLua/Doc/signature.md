## 源代码签名的用处

可以防止文件传输的过程被黑客篡改。

## xLua的签名功能的使用

* 用Tools/KeyPairsGen.exe生成公私钥对，key_ras文件保存的是私钥，key_ras.pub保存的是公钥，这两个文件请妥善保存，私钥关系到游戏安全，请做好保密工作；
* 用Tools/FilesSignature.exe对源代码进行签名：
  * key_ras文件要放到执行目录；
  * 参数是源目录和目标目录，这个工具会自动把源目录及其子目录下所有lua后缀的文件签名，按同样的目录结构放到目标目录下；
* 通过SignatureLoader对自己原有的CustomLoader包装后使用；
  * SignatureLoader的构造函数有两个，一个是公钥，也就是key_ras.pub里头的内容，一个是原来的Loader；
