# AutomaticDynamicBone
基于https://github.com/SPARK-inc/SPCRJointDynamics ,一个可以自动生成具有物理效果头发和裙子的插件.  
## 特性

- 采用 unity Job System + Burst compiler,采用指针写的物理底层,拥有着**及其高的优化程度!**
- 支持除了WebGL以外**所有平台**
- 无需任何复杂的添加与操作,通过关键词识别与humanoid识别,只需要三分钟学习就可以**一键生成**你想要的bone与collider!
- **完全自动化的脚本**允许你在runtime过程中生成整套的bone与collider,是的,你甚至不需要任何操作就可以完整套物理的生成!
- **完整的内部代码**以及良好的代码工作,内置的debug辅助措施能让你更好的查看代码的工作情况!
- 极其高的精确程度(划掉)**无限制的迭代次数**!只要你电脑能够撑住,就能有多么精确!
- **良好的迭代计算**!不会震颤的运动轨迹以及对高速运动的有效解决方案.
- **免费**以及作者被dynaimc bone坑走了15美刀.
- 作者是模型白嫖怪(划掉)MMD友好程度**极高!**

## 要求
- Unity2018.2或更高
- 要求 [IL2CPP][.NET4x][Allow 'unsafe' Code]
- 安装 [Burst] package
- 安装 [Jobs] package(preview)
- 安装 [Collection] package(preview)
- 安装 [Mathmatic] package(preview)

- 更多详情请参见[wiki](https://github.com/OneYoungMean/AutomaticDynamicBone/wiki)  
![2](https://github.com/OneYoungMean/AutomaticDynamicBone/blob/master/Manual%20GIF/A0.gif)  
`看见上面的物理计算没,相同的模型我的6700HQ能同时64个还能再60帧!`

