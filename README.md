写在开头:**您可能需要科学上网才能看到GIF图!**  
噫,好,我中了!(嗝屁)   
如果您实在翻不了墙,下载还贼慢,你可以选择在[此处下载](https://pan.baidu.com/s/1Gb7lKFFqKpjqfD9InBA2tA 
)提取码：92ks

# AutomaticDynamicBone
基于https://github.com/SPARK-inc/SPCRJointDynamics ,一个可以根据骨骼布料,自动生成具有物理效果头发和裙子的插件.  
**ADB目前更新到0.2版本,请还在0.1的同学记得更新哦.**
## 新功能说明
~ 修改了setting文件的选项,详情参考setting文件说明书. **注意,更新前请备份你已经设置好的setting文件!** 
~ 增加了实时调试功能,现在你可以在调试中使用force fresh point 功能强制更新所有点的数据,这意味着你可以实施调试setting文件的效果了.
~ 删除了一些开倒车的功能,增加了自定义的weight.  
~ 重置了所有的setting文件,现在它们不会再坑爹了!  
## 特性

- 采用 unity Job System + Burst compiler,采用指针写的物理底层,拥有着**极其强悍的优化程度!**[详情](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki/Q&A#q%E6%80%A7%E8%83%BD%E6%96%B9%E9%9D%A2%E5%85%B7%E4%BD%93%E6%80%8E%E4%B9%88%E6%A0%B7)  
- 支持除了WebGL以外**所有平台**
- **极其低的学习曲线!** 作者已经帮你们把门槛踏平了!无需任何复杂的添加与操作,通过关键词识别与humanoid识别,只需要三分钟学习就可以**一键生成**你想要的bone与collider!
- **完全自动化的脚本**允许你在runtime过程中生成整套的bone与collider,是的,你甚至不需要任何操作就可以完整套物理的生成!
- **完整的内部代码**以及良好的代码工作,内置的debug辅助措施能让你更好的查看代码的工作情况!
- **高度的可定制性**可以自由的组合出你想要的的物理特性!
- 极其高的精确程度(划掉)**无限制的迭代次数**!只要你电脑能够撑住,就能有多么精确!
- **良好的迭代计算**!只需四次迭代就能获得逼真的物理效果!同时还有不会震颤的运动轨迹以及对高速运动的有效解决方案.
- **免费**以及**作者被dynaimc bone坑走了15美刀.**
- 作者是MMD模型白嫖怪(划掉)MMD友好程度**极高!**
![2](https://github.com/OneYoungMean/AutomaticDynamicBone/blob/master/Manual%20GIF/A0.gif)  
`看见上面的物理计算没，相同的模型我的6700HQ能同时64个还能再60帧！  
![2](https://s2.ax1x.com/2020/02/29/3yRc8g.gif)

## 要求
- Unity2019.1f10或更高版本测试完毕, 2018.3以下的版本似乎不行
- 要求 [IL2CPP][.NET4x][Allow 'unsafe' Code]
- 需要 [Burst] package
- 需要 [Jobs] package(preview)
- 需要 [Collection] package(preview)
- 需要 [Mathmatic] package(preview)

## 快速开始

1. 在脚本中找到**ADBRuntimeController**,添加到你想要添加的目标/目标父物体上.
2. 检查目标需要添加物理效果的骨骼,通常这类骨骼名字都会包含一个**固定的关键词**,比如hair,skirt,你需要把关键词写入到`NameKeyWord`中.
3. 按下`Generate Point`,并在底下找到isDebug勾选,如果一切顺利的话,你可以看到几条彩色的线和点,这些就是识别到的数据,是查看是否有遗漏的/错误的骨骼,并通过`BlackListTranform`和`BlackListKeyword`来排除自己不需要的骨骼.  
4. 运行游戏,一切已经就绪,晃动你的目标以查看效果,就是这么快XD!
- 更多详情请参见[wiki](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki)  
![3](https://github.com/OneYoungMean/Automatic-DynamicBone/blob/master/Manual%20GIF/A3.gif)   

