写在开头:**您可能需要科学上网才能看到GIF图!**  
目前尚未进入稳定版本,请选择性参考本插件XD
如果您实在翻不了墙，下载还贼慢，你可以选择在[此处下载](https://gitee.com/OneYoungMean/Automatic-DynamicBone)

[English version manual](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki/English-version-manual )  
# AutomaticDynamicBone

基于https://github.com/SPARK-inc/SPCRJointDynamics ,一个可以根据骨骼布料,自动生成具有物理效果头发和裙子的插件.  
- **当前版本:0.5preview 最后更新日期:20/7/24**
- **注意,更新前请备份你已经设置好的setting文件!**  
## 新功能说明
- **更新了示例,调试了大部分功能,基本上确认项目暂时稳定**
- **重新写了一遍底层,效果提升4倍,现在允许你只迭代一次就能获得比较完整的效果**
- **为了达到尽可能高效率的性能,脚本只能通过命名添加骨骼,该操作暂时有些反人类,我会尽快解决这个问题**
- 	<s>修改了重力的作用方式</s>**又改回去了**
- **修改了freeze属性的作用方式,你可以通过调整重力来修改freeze的位置方向了**
- **处理了数不胜数的bug,以及即将到来的骨骼细分黑科技**
- **添加了一个新的免费示例模型,感谢Kafuji,以及[在此处获取更多的模型](https://fantia.jp/fanclubs/3967)**
## 特性
- **无需(但兼容)ECS与Dots!** 我们并没有采用ECS系统!除了unity的多线程jobs系统,插件并没有额外涉及任意ECS的内容!你不用担心额外的jobs系统其对你的项目造成影响!

- **专为高性能定制**尽可能脱离对于monobehavior的操作,采用 unity Job System + Burst compiler作为基础,采用指针写的物理底层,使用多线程进行计算与修改transfrom,拥有着**极其强悍的优化程度!**[详情](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki/Q&A#q%E6%80%A7%E8%83%BD%E6%96%B9%E9%9D%A2%E5%85%B7%E4%BD%93%E6%80%8E%E4%B9%88%E6%A0%B7)  

- 支持除了WebGL以外**所有平台**  

- **极其低的学习曲线!** 作者已经帮你们把门槛踏平了!无需任何复杂的添加与操作,通过关键词识别与humanoid识别,只需要三分钟学习就可以**一键生成**你想要的bone与collider!

- **良好的报错系统!** 任何不正确的操作都会给出相关的提示,作者已经帮你们把能犯的错误犯过了!  

- **适应runtime的脚本!** 无需复杂的操作,只要简单设置参数,就能允许你在runtime过程中生成整套的物理特性,是的,你甚至不需要任何操作就可以完整套物理的生成!  

- **高度自由的物理与运行时保存系统!** 提供各种参数,可以自由的组合出你想要的的物理特性!你无需反复调试这些物理参数,因为你可以随时在runtime过程中修改并保存他们!  

- **独特的迭代与除颤机制!** 我们通过细分物体的运行轨迹,在一帧内同时计算多个细分位置的受力情况并加以综合,通常只需要四次你就能获得预期的效果,只要你迭代的足够多,你就能获得无限接近稳定的物理!  

- **完整且高效的Collider系统!** 支持球体,胶囊体,立方体的碰撞;支持杆件/点与collider的碰撞;无需创建transfrom,你可以在交互界面实现偏移,旋转等效果!;同时,该脚本提供了一套完整的collider过滤机制,同时也提供了近似估计的功能,一切都是为了让你尽可能的快!  

- **简洁的操作界面!** 是的,我们已经将大部分能够优化的操作界面已经优化掉了,现在不会再有多余的选项出现,并且你可以直接在inspector看到统计的数据.  

- **完整的内部源码!** 不打包dll,提供所有的运行细节以及大量的注释!你可以任意定修改某一部分,已获得想要的物理效果与特殊性质,并且大可不必担心随之而来的耦合问题!  

- **免费!** 以及作者被dynaimc bone坑走了15美刀.** 并且<s>作者是MMD模型白嫖怪</s>MMD友好程度**极高!**

- **作者长期在线!**有issue必回!包君满意!

![2](https://github.com/OneYoungMean/AutomaticDynamicBone/blob/master/Manual%20GIF/A0.gif)  
 
![2](https://s2.ax1x.com/2020/02/29/3yRc8g.gif)

## 要求

- Unity2018.4及以上,除webGL外所有支持unity jobs的平台.  

## 快速入门
如果你并不想下载项目附带的example压缩包,你可以按以下操作快速查看运行效果!  

1. 在脚本中找到**ADBRuntimeController**,添加到你想要添加的目标/目标父物体上.
2. 检查目标需要添加物理效果的骨骼,通常这类骨骼名字都会包含一个**固定的关键词**,比如hair,skirt,你需要把关键词写入到`NameKeyWord`中.
3. 按下`Generate Point`,并在底下找到isDebug勾选,如果一切顺利的话,你可以看到几条彩色的线和点,这些就是识别到的数据,是查看是否有遗漏的/错误的骨骼,并通过`BlackListTranform`和`BlackListKeyword`来排除自己不需要的骨骼.  
4. 运行游戏,一切已经就绪,晃动你的目标以查看效果,就是这么快XD!
- 更多详情请参见[wiki](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki)  
![3](https://github.com/OneYoungMean/Automatic-DynamicBone/blob/master/Manual%20GIF/A3.gif)   

