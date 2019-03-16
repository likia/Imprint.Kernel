# Imprint开发核心类库

* Core 核心功能模块
    * EventManager 全局事件管理器
    * ICipher 加密类接口，所有加密算法规范化标准
    * IRedial 重播类接口，标准化重播换ip操作
    * LogManager 全局日志管理器
    * LoopPicker 循环拾取器
    * Picker 拾取器抽象类
    * RandomPicker 随机拾取器
    * ResourceManager 全局资源管理器
    * SaveManager 全局序列化管理器
    * SettingManager 全局设置管理器
* Imaging 图像处理
    * EffImage 高效图片处理类， 用指针直接读取位图数据，更快操作图像像素数据，并封装了多种常用的验证码处理方法如卷积滤镜，二值化，灰度化
* Network 网络模块
    * WebConnection 封装单次http请求，使用Handler自动转换数据
    * WebSession 封装一个http会话，自动处理会话中的cookie
* Security 加密类
    * AES 实现AES加密
    * RSA 实现RSA加密
    * SecureHash 实现常用哈希算法
* Task 多线程模块
    * Dispatcher 线程池调度器
    * Ticker 基于调度器实现定时任务
* Trinity 基于卷积神经网络实现的验证码识别分类器， 网络结构可以通过Schema自动加载配置文件创建， 默认网络结构为62个输出类别的Lenet-5
    * Recognizer 识别器
* Util
    * JSON json解析处理 （from：https://github.com/neuecc/DynamicJson）
    * Misc 杂项
    * StrHelper 封装常用字符串操作