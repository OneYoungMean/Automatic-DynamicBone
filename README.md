写在开头:**示例自带的脚本有bug,报错的话请删除**  
如果您实在翻不了墙，下载还贼慢，你可以选择在[此处下载](https://gitee.com/OneYoungMean/Automatic-DynamicBone)  
[English version manual](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki/English-version-manual)  

# AutomaticDynamicBone

基于https://github.com/SPARK-inc/SPCRJointDynamics ,基于unity jobs 多线程系统,一个可以根据骨骼布料,自动生成具有物理效果头发和裙子,用于代替dynamicBone的**unity骨骼布料仿真插件**.  
此外,缅怀作者被dynamic bone坑走了**15美刀**  

- **当前版本:0.8preview 最后更新日期:20/7/31**  
- **当前版本更新频繁,请耐心等待**   

## 新功能说明
- **即将到来的新介绍**   

- **中文面板!中文面板!中文面板!**  
- **添加了一种全新识别节点的方式,它有点类似于Springbone,现在添加骨骼不会那么反人类了!**
- **修正了强制主线程完成所有任务的问题,现在你可以异步等待结果完成**
- **添加了轨迹优化选项,这个选项会改变一部分物理特性,这可能会让头发运动显得不那么自然,但是却能让它在高速运动下表现的更好**   
- **添加了重力轴跟随角色旋转的功能,允许你制作你的反重力裙子**
- **添加了节点的半径选项(球体),现在节点与碰撞体碰撞的时候回考虑到节点的半径了!**
- **添加了5种新的碰撞体工作模式,请在wiki当中查看他们的介绍!**
- **修改了虚拟点的存在形式与工作方式,你现在可以找到这些点的transfrom并查看他们的工作情况了**  
- **修改了一部分参数的权重,现在参数的调整建议都是0~1**  
- **修改了freeze属性的作用方式,你可以通过调整重力来修改freeze的位置方向了**  

- **一些选项的优化**  


- **添加了一个新的免费示例模型,感谢Kafuji,以及[在此处获取更多的模型](https://fantia.jp/fanclubs/3967)**  

## 特性

- **无需(但兼容)ECS与Dots!** 我们并没有采用ECS系统!除了unity的多线程jobs系统,插件并没有额外涉及任意ECS的内容!你不用担心额外的jobs系统其对你的项目造成影响!

- **专为高性能定制**尽可能脱离对于monobehavior的操作,尽可能减少GC以及针对物理效果的优化,采用 unity Job System + Burst compiler作为基础,采用指针写的物理底层,使用多线程进行计算与修改transfrom,拥有着**极其强悍的优化程度!**[详情](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki/Q&A#q%E6%80%A7%E8%83%BD%E6%96%B9%E9%9D%A2%E5%85%B7%E4%BD%93%E6%80%8E%E4%B9%88%E6%A0%B7)  

- 支持除了WebGL以外**所有平台**  

- **极其低的学习曲线!** 作者已经帮你们把门槛踏平了!无需任何复杂的添加与操作,通过关键词识别与humanoid识别,只需要三分钟学习就可以**一键生成**你想要的bone与collider!

- **良好的报错系统!** 任何不正确的操作都会给出相关的提示,作者已经帮你们把能犯的错误犯过了!  

- **适应runtime的脚本!** 无需复杂的操作,只要简单设置参数,就能允许你在runtime过程中生成整套的物理特性,是的,你甚至不需要任何操作就可以完整套物理的生成!  

- **高度自由的物理与运行时保存系统!** 提供各种参数,可以自由的组合出你想要的的物理特性!你无需反复调试这些物理参数,因为你可以随时在runtime过程中修改并保存他们!  

- **独特的迭代与除颤机制!** 我们通过细分物体的运行轨迹,在一帧内同时计算多个细分位置的受力情况并加以综合,通常只需要四次你就能获得预期的效果,只要你迭代的足够多,你就能获得无限接近稳定的物理!  

- **完整且高效的Collider系统!** 支持球体,胶囊体,立方体的碰撞;支持杆件/点与collider的碰撞;无需创建transfrom,你可以在交互界面实现偏移,旋转等效果!;同时,该脚本提供了一套完整的collider过滤机制与距离近似估计的算法,大大提高了碰撞的运行效率,一切都是为了让你尽可能的快!  

- **独特的黑科技!** **一键生成人物一整套Collider,可定制的Collider对撞规则,动态实时的性能调节**以及,你甚至可以**在编译好的程序中给AssetBundle包当中的角色**[添加该脚本](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki/ADBRuntimeController%E4%BB%8B%E7%BB%8D#%E5%A6%82%E4%BD%95%E5%9C%A8runtime%E7%9A%84%E6%97%B6%E5%80%99%E6%B7%BB%E5%8A%A0%E8%AF%A5%E8%84%9A%E6%9C%AC)**及时生成物理!**

- **简洁的操作界面!** 是的,我们已经将大部分能够优化的操作界面已经优化掉了,现在不会再有多余的选项出现,并且你可以直接在inspector看到统计的数据.  

- **完整的内部源码!** 不打包dll,提供所有的运行细节以及大量的注释!你可以任意定修改某一部分,已获得想要的物理效果与特殊性质,并且大可不必担心随之而来的耦合问题!  

- **免费!** 以及作者被dynaimc bone坑走了15美刀.** 并且<s>作者是MMD模型白嫖怪</s>MMD友好程度**极高!**

- **作者长期在线!** 有issue必回!包君满意!

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
![3](https://github.com/OneYoungMean/Automatic-DynamicBone/blob/master/Manual%20GIF/A3.gif)   

### 说明书

[更多详情请参见wiki](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki) 

### 最后,如果你喜欢本项目记得给本项目star!
```C#
[省略掉的吐槽很辛苦的话]
[省略掉的吐槽自己真的很穷的话]
[省略掉的小声BB的话]
有没有愿意收留实习生的,弱小,无助,可怜,还没钱QAQ
```
