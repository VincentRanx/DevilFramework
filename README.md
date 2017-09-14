# DevilFramework
一些Unity开发中常用的方法和组件
Assets 目录包含了Unity中的常用脚本。
其中 Assets/Core中包含了核心代码，提供了很多常用的方法和工具，如运行时脚本程序虚拟机，有限状态机，行为树（待完成）等。
Assets/Core/RTSVM 实现了一个简单的运行时脚本程序虚拟机，需要时可以添加一个叫RTSUnityRuntime的组件实现虚拟机运行。
Assets/Command提供了一个便于Unity编辑器运行时输入脚本来完成测试或程序化编辑的操作。
