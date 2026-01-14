# NV.CT.SystemInterface.MRSIntegration 分析文档

## 1. 概述

`NV.CT.SystemInterface.MRSIntegration` 模块遵循接口与实现分离的设计原则，由两个核心项目组成：

-   **`NV.CT.SystemInterface.MRSIntegration.Contract` (合约项目)**: 定义了服务和数据模型的“契约”。它只包含接口（Interfaces）和数据传输对象（DTOs/Models），规定了“能做什么”和“用什么数据”，但不包含任何业务逻辑。
-   **`NV.CT.SystemInterface.MRSIntegration.Impl` (实现项目)**: 包含了 `Contract` 项目中定义的接口的具体实现。所有业务逻辑、数据处理以及与底层 `FacadeProxy` 的交互都在这里完成。

这种模式实现了**松耦合**，使得系统的其他部分可以只依赖于 `Contract`（抽象），而无需关心 `Impl`（具体实现）的细节，极大地提高了系统的可维护性和灵活性。

---

## 2. 数据模型 (Data Models)

这些类定义了在服务接口之间传递的数据结构，位于 `SystemInterface/MRSIntegration.Contract/Models/`。

| 类名 | 描述 |
| --- | --- |
| `DevicePart.cs` | 定义了 CT 设备硬件组件的基础类和模型。包括 `DevicePart` (基类), `Gantry` (机架), `Table` (检查床), `Tube` (球管), `Detector` (探测器) 等。 |
| `DeviceSystem.cs` | 一个聚合模型，代表了整个 CT 设备的实时状态。它包含了所有主要的设备部件 (`Gantry`, `Table`, `Detector` 等) 和系统级的状态信息。 |
| `DoseEstimateParam.cs`| 用于封装计算预估放射剂量所需参数的结构体，例如扫描选项、kV、mA、扫描长度等。 |

---

## 3. 接口与实现 (Interfaces & Implementations)

下表详细列出了 `Contract` 项目中定义的所有接口、它们的功能描述，以及在 `Impl` 项目中对应的实现类和核心逻辑。

| 接口 (Contract) | 实现类 (Impl) | 描述与核心逻辑 |
| --- | --- | --- |
| `ICTBoxStatusService` | `CTBoxStatusService` | **功能**: 提供 CTBox 的状态。<br>**逻辑**: 订阅 `IRealtimeStatusProxyService` 的 `CycleStatusChanged` 事件，从中获取 CTBox 的状态并通知订阅者。 |
| `IComponentStatusProxyService`|`ComponentStatusProxyService`| **功能**: 获取设备各组件的固件版本信息。<br>**逻辑**: 直接调用 `ComponentStatusProxy.Instance` 单例的方法来获取采集卡、探测器及其他组件的固件版本。 |
| `IControlBoxStatusService` | `ControlBoxStatusService` | **功能**: 提供操作盒（ControlBox）的状态。<br>**逻辑**: 与 `CTBoxStatusService` 类似，通过订阅 `IRealtimeStatusProxyService` 的周期性状态事件来更新自身状态。 |
| `IDetectorTemperatureService`|`DetectorTemperatureService`| **功能**: 管理探测器的温度状态。<br>**逻辑**: 从 `IRealtimeStatusProxyService` 获取温度是否正常的状态，并提供设置目标温度的接口，该接口通过调用 `DeviceInteractProxy.Instance` 来实现。 |
| `IDoorStatusService` | `DoorStatusService` | **功能**: 报告扫描间门是否关闭。<br>**逻辑**: 订阅 `IRealtimeStatusProxyService` 的 `CycleStatusChanged` 事件，并根据事件数据更新门的开关状态。 |
| `IDoseEstimateService` | `DoseEstimateService` | **功能**: 在扫描前预估放射剂量 (CTDI)。<br>**逻辑**: 使用 `CTDICalculateService` 辅助类，根据传入的 `DoseEstimateParam`（扫描参数），调用相应的剂量计算方法。 |
| `IFrontRearCoverStatusService`|`FrontRearCoverStatusService`| **功能**: 报告机架前后盖是否关闭。<br>**逻辑**: 同样通过订阅 `IRealtimeStatusProxyService` 的周期性事件来获取状态。 |
| `IHeatCapacityService` | `HeatCapacityService` | **功能**: 提供 X 射线球管的热容量信息。<br>**逻辑**: 从 `IRealtimeStatusProxyService` 的 `CycleStatusChanged` 事件中提取球管的热容量数据。 |
| `IOfflineConnectionService` | `OfflineConnectionService` | **功能**: 管理与离线重建服务器的连接状态。<br>**逻辑**: 封装了 `IOfflineProxyService`，监听其连接和错误事件，并向上层提供更简洁的连接状态通知。 |
| `IOfflineProxyService` | `OfflineProxyService` | **功能**: 对 `OfflineMachineTaskProxy` 的底层封装。<br>**逻辑**: 初始化并管理与离线服务器的连接，将来自 `OfflineMachineTaskProxy.Instance` 的底层事件（如连接、错误、任务状态）转发给订阅者。 |
| `IOfflineTaskProxyService` | `OfflineTaskProxyService` | **功能**: 提供离线任务（重建、后处理）的管理能力。<br>**逻辑**: 调用 `OfflineMachineTaskProxy.Instance` 来执行创建、开始、停止、删除任务等命令，并监听 `IOfflineProxyService` 的事件来更新任务状态和进度。 |
| `IRealtimeConnectionService`|`RealtimeConnectionService`| **功能**: 管理与实时设备和重建服务的连接状态。<br>**逻辑**: 封装 `IRealtimeProxyService`，为其底层的设备连接和重建服务连接状态变更提供独立的、更清晰的事件通知。 |
| `IRealtimeProxyService` | `RealtimeProxyService` | **功能**: 对 `AcqReconProxy` 的核心底层封装。<br>**逻辑**: 在服务初始化时连接 MRS 的各项实时服务，并将来自 `AcqReconProxy.Instance` 的所有实时事件（系统状态、图像保存、错误等）直接向上层转发。 |
| `IRealtimeReconProxyService`|`RealtimeReconProxyService`| **功能**: 负责执行实时扫描操作。<br>**逻辑**: 调用 `AcqReconProxy.Instance` 的 `StartScan`、`AbortScan` 等核心扫描命令，并监听 `IRealtimeProxyService` 的事件来跟踪扫描状态和接收图像。 |
| `IRealtimeStatusProxyService`|`RealtimeStatusProxyService`| **功能**: 将底层的实时事件转换成结构化的设备状态信息。<br>**逻辑**: 订阅 `IRealtimeProxyService` 的 `CycleStatusChanged` 事件，并将事件中的海量数据解析并映射到结构化的 `DeviceSystem` 模型中，供其他服务使用。 |
| `IRealtimeVoiceService` | `RealtimeVoiceService` | **功能**: 管理设备上的语音播放（如病人指令）。<br>**逻辑**: 通过调用 `DeviceInteractProxy.Instance` 的接口来实现添加、删除、播放语音文件等功能。 |
| `ISelfCheckingProxyService` | `SelfCheckingProxyService` | **功能**: 管理设备的自检流程。<br>**逻辑**: 封装 `SelfCheckProxy.Instance`，用于获取自检结果和监听自检状态的变化。 |
| `IShutdownProxyService` | `ShutdownProxyService` | **功能**: 提供关闭和重启系统不同部分（设备、离线机）的功能。<br>**逻辑**: 封装 `ShutdownProxy.Instance`，根据指定的 `ShutdownScope` 向其发送关机或重启指令。 |
| `ITablePositionService` | `TablePositionService` | **功能**: 管理检查床的位置和移动。<br>**逻辑**: 从 `IRealtimeStatusProxyService` 获取床的当前位置，并通过 `AcqReconProxy` 向设备发送移动指令。该服务包含了复杂的业务逻辑，用于校验移动的安全性（例如，床的高度与伸出长度的限制关系）。 |
