写在开头:**您可能需要科学上网才能看到GIF图!**
如果您实在翻不了墙,下载还贼慢,你可以选择在[此处下载](https://pan.baidu.com/s/1Ku0kP6xLLpuFThX4WeDr4A)  提取码：10we 

# AutomaticDynamicBone
基于https://github.com/SPARK-inc/SPCRJointDynamics ,一个可以根据骨骼布料,自动生成具有物理效果头发和裙子的插件.  
**ADB目前更新到0.1版本,请还在原始版本的同学记得更新哦.**

## 特性

- 采用 unity Job System + Burst compiler,采用指针写的物理底层,拥有着**极其强悍的优化程度!**[详情](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki/Q&A#q%E6%80%A7%E8%83%BD%E6%96%B9%E9%9D%A2%E5%85%B7%E4%BD%93%E6%80%8E%E4%B9%88%E6%A0%B7)  
- 支持除了WebGL以外**所有平台**
- **极其低的学习曲线!** 作者已经帮你们把门槛踏平了!无需任何复杂的添加与操作,通过关键词识别与humanoid识别,只需要三分钟学习就可以**一键生成**你想要的bone与collider!
- **完全自动化的脚本**允许你在runtime过程中生成整套的bone与collider,是的,你甚至不需要任何操作就可以完整套物理的生成!
- **完整的内部代码**以及良好的代码工作,内置的debug辅助措施能让你更好的查看代码的工作情况!
- **高度的可定制性**可以自由的组合出你想要的的物理特性!
- 极其高的精确程度(划掉)**无限制的迭代次数**!只要你电脑能够撑住,就能有多么精确!
- **良好的迭代计算**!只需四次迭代就能获得逼真的物理效果!同时还有不会震颤的运动轨迹以及对高速运动的有效解决方案.
- **免费**以及作者被dynaimc bone坑走了15美刀.
- 作者是MMD模型白嫖怪(划掉)MMD友好程度**极高!**
![2](https://github.com/OneYoungMean/AutomaticDynamicBone/blob/master/Manual%20GIF/A0.gif)  
`看见上面的物理计算没,相同的模型我的6700HQ能同时64个还能再60帧!`
## 要求
- Unity2018.2或更高
- 要求 [IL2CPP][.NET4x][Allow 'unsafe' Code]
- 需要 [Burst] package
- 需要 [Jobs] package(preview)
- 需要 [Collection] package(preview)
- 需要 [Mathmatic] package(preview)

- 更多详情请参见[wiki](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki)  


