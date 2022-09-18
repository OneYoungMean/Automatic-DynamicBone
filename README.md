# License
[MIT License](https://github.com/OneYoungMean/Automatic-DynamicBone/blob/master/LICENSE)
# 写在开头  
感谢大家的支持，700 star达成~    
该项目前与[UnityBVA](https://github.com/bilibili/UnityBVA)联动中，新增了UI优化并针对MMD转换与VRM转换做了补丁，希望大家能够多多支持~  
![[](https://imgtu.com/i/XGsPRx) ](https://s1.ax1x.com/2022/06/01/XGsPRx.png)  
新版本可能存在不稳定的情况，如果你遇到了某些奇怪问题，请提交issue。  
作者去肝论文了，预计四月份才会出关，期间回复较慢的话请谅解（躺）
* 如果您是第一次接触该项目,您可以选择访问[项目的说明](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki/Automatic-Dynamic-Bone-%E8%AF%B4%E6%98%8E%E4%B9%A6)  

- **当前版本:2.0.0preview 最后更新日期:22/2/27**  
- **预计将会全部英文化(包括说明书),请各位备好谷歌翻译(笑)**

# 2.0.0更新点速览    
* **~~性能优化~~安卓上多个角色（5+）的性能测试通过!**
* **加入了SpringBone的骨骼原理,更加丰富的可调物理参数!更加丝滑的头发!**
* **全新的Editor界面!更加丰富的操作面板和选项!**  
* **物理骨骼生成现在单独作为一个工具**
* **碰撞体生成现在单独作为一个工具**
* **单线程/多线程/并行可切换**
* **支持Scale调整-无论是collider还是角色!**
* Fix BUG,噢,无穷无尽的bug.
* 一些新特性,他们太细小了,我甚至没有篇幅来描述他们.

## 更新注意事项
**请确保您的mathmatica版本高于等于1.2.1**  
**如果遇到卡顿问题,请尝试在jobs菜单下关闭SafelyCheck与enableJobDebuger选项**  

# AutomaticDynamicBone 
![](https://z3.ax1x.com/2021/09/29/44E1Gn.png) 

**unity骨骼布料仿真插件**. 
* 基于https://github.com/SPARK-inc/SPCRJointDynamics
* 基于unity jobs 多线程系统
* 一款轻量级的开源插件,可以根据骨骼自动生成具有物理效果头发和裙子,用于代替dynamicBone插件功能
* 此外,缅怀作者被dynamic bone坑走了**15美刀**  
* ![](https://z3.ax1x.com/2021/09/29/45i1LF.gif)
![](https://z3.ax1x.com/2021/09/29/44EJMV.gif)
![](https://z3.ax1x.com/2021/09/29/44Kfn1.gif)  

[更多演示视频](https://www.bilibili.com/video/BV1wP4y187xE/)  

## 特性

- **针对高性能进行优化**尽可能脱离对于monobehavior的操作,尽可能减少GC以及针对物理效果的优化,采用 unity Job System + Burst compiler作为基础,采用指针写的物理底层,使用多线程进行计算与修改transfrom,拥有着**极其强悍的优化程度!**[详情](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki/Q&A#q%E6%80%A7%E8%83%BD%E6%96%B9%E9%9D%A2%E5%85%B7%E4%BD%93%E6%80%8E%E4%B9%88%E6%A0%B7)  

- **无需(但兼容)ECS与Dots!** 我们并**没有**采用ECS系统!除了unity的多线程jobs系统,你不用担心额外的jobs系统其对你的项目造成影响!并支持除了WebGL以外**所有平台**  

- **极其低的学习曲线!** 作者已经帮你们把门槛踏平了!无需任何复杂的添加与操作,通过关键词识别与humanoid识别,只需要三分钟学习就可以**一键生成**你想要的bone与collider!

- **良好的报错系统!** 任何不正确的操作都会给出相关的提示,作者已经帮你们把能犯的错误犯过了!  

- **适应runtime的脚本!** 无需复杂的操作,只要简单设置参数,就能允许你在runtime过程中生成整套的物理特性,是的,你甚至不需要任何操作就可以完整套物理的生成!  

- **高度自由的物理与运行时保存系统!** 提供各种参数,可以自由的组合出你想要的的物理特性!你无需反复调试这些物理参数,因为你可以随时在runtime过程中修改并保存他们!  

- **独特的迭代与除颤机制!** 我们通过细分物体的运行轨迹,在一帧内同时计算多个细分位置的受力情况并加以综合,通常只需要四次你就能获得预期的效果,只要你迭代的足够多,你就能获得无限接近稳定的物理!  

- **完整且高效的Collider系统!** 支持球体,胶囊体,立方体的碰撞;支持杆件/点与collider的碰撞;无需创建transfrom,你可以在交互界面实现偏移,旋转等效果!;同时,该脚本提供了一套完整的collider过滤机制与距离近似估计的算法,大大提高了碰撞的运行效率,一切都是为了让你尽可能的快!  

- **简洁的操作界面!** 是的,我们已经将大部分能够优化的操作界面已经优化掉了,现在不会再有多余的选项出现,并且你可以直接在inspector看到统计的数据.  

- **完整的内部源码!** 不打包dll,提供所有的运行细节以及大量的注释!你可以任意定修改某一部分,已获得想要的物理效果与特殊性质,并且大可不必担心随之而来的耦合问题!  

- **免费!** 以及作者被dynaimc bone坑走了15美刀.** 并且<s>作者是MMD模型白嫖怪</s>MMD友好程度**极高!**

- **作者长期在线!** 有issue必回!包君满意!

## 要求

- Unity2018.4及以上,除webGL外所有支持unity jobs的平台.  
***

## 被引
* [UnityBVA](https://github.com/bilibili/UnityBVA)

## 快速开始

施工中...

### 说明书

施工中...

### 最后,如果你喜欢本项目记得给本项目star!
```C#
[省略掉的吐槽很辛苦的话]
[省略掉的吐槽自己真的很穷的话]
[省略掉的小声BB的话]
加油嗷~
```

