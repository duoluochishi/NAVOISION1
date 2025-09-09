use db_mcs;

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for t_dose_check
-- ----------------------------
DROP TABLE IF EXISTS `t_dose_check`;
CREATE TABLE `t_dose_check` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '检查ID(内部)',
  `InternalPatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '患者ID(内部)',
  `FrameOfReferenceId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `MeasurementId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `ScanId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `DoseCheckType` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `WarningCTDI` float NOT NULL DEFAULT '0' COMMENT 'CTDI配置阈值，配合DoseCheckType',
  `WarningDLP` float NOT NULL DEFAULT '0' COMMENT 'DLP配置阈值，配合DoseCheckType',
  `CurrentCTDI` float NOT NULL DEFAULT '0' COMMENT 'CTDI当前值，配合DoseCheckType',
  `CurrentDLP` float NOT NULL DEFAULT '0' COMMENT 'DLP当前值，配合DoseCheckType',
  `Operator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '操作人',
  `Reason` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '原因',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='检查剂量检测原因表';

-- ----------------------------
-- Table structure for t_job_task
-- ----------------------------
DROP TABLE IF EXISTS `t_job_task`;
CREATE TABLE `t_job_task` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `WorkflowId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `InternalPatientID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '患者ID(内部)',
  `InternalStudyID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查ID(内部)',
  `JobType` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT '' COMMENT 'job任务类型：导入,导出, 归档',
  `JobStatus` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT '任务状态',
  `Parameter` json DEFAULT NULL COMMENT 'json参数',
  `Priority` tinyint NOT NULL DEFAULT '5',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录创建时间',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人',
  `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间',
  `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人',
  `StartedDateTime` datetime DEFAULT NULL,
  `FinishedDateTime` datetime DEFAULT NULL,
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ----------------------------
-- Table structure for t_patient
-- ----------------------------
DROP TABLE IF EXISTS `t_patient`;
CREATE TABLE `t_patient` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `PatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '病人ID',
  `PatientName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人姓名',
  `PatientSex` int NOT NULL DEFAULT '0' COMMENT '性别',
  `PatientBirthDate` datetime DEFAULT NULL COMMENT '出生日期',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '存储状态：0 正常; 1已删除',
  `CreateTime` datetime DEFAULT NULL COMMENT '记录创建时间',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人',
  `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间',
  `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人', 
  `PatientComments` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人注释',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='患者信息表';

-- ----------------------------
-- Table structure for t_permissions
-- ----------------------------
DROP TABLE IF EXISTS `t_permissions`;
CREATE TABLE `t_permissions` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Code` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '代码',
  `Name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '名称',
  `Description` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '描述',
  `Category` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '权限类别',
  `Level` smallint NOT NULL DEFAULT '0' COMMENT 'Normal,Service,Admin',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '标记删除',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='权限表';

-- ----------------------------
-- Table structure for t_protocol_usage_records
-- ----------------------------
DROP TABLE IF EXISTS `t_protocol_usage_records`;
CREATE TABLE `t_protocol_usage_records` (
  `ID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `StudyID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '关联Study检查表StudyID',
  `ProtocolId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '关联协议编号',
  `loginID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '登录账号',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  PRIMARY KEY (`ID`),
  KEY `Index_ProtocolID` (`ProtocolId`),
  KEY `Index_StudyID` (`StudyID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='协议使用统计表';

-- ----------------------------
-- Table structure for t_recon_task
-- ----------------------------
DROP TABLE IF EXISTS `t_recon_task`;
CREATE TABLE `t_recon_task` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '主键ID',
  `InternalStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查id',
  `InternalPatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人ID',
  `FrameOfReferenceUid` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'FOR id',
  `ScanId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '关联扫描任务表扫描任务ID',
  `ReconId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '关联协议中重建任务ID',
  `IsRTD` bit(1) DEFAULT NULL COMMENT '是否RTD',
  `SeriesNumber` int DEFAULT '0' COMMENT '图像序列编号',
  `SeriesDescription` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '序列描述',
  `WindowWidth` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '窗宽',
  `WindowLevel` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '床位',
  `ReconStartDate` datetime DEFAULT NULL COMMENT '重建开始时间',
  `ReconEndDate` datetime DEFAULT NULL COMMENT '重建结束时间',
  `IsDeleted` bit(1) DEFAULT b'0' COMMENT '是否删除',
  `Description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '描述',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人',
  `TaskStatus` int DEFAULT '0' COMMENT '任务状态 0 未开始 1待重建 2重建中 3重建完成 4重建失败',
  `Remark` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '备注',
  `IssuingParameters` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '下发参数',
  `ActuralParameters` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '实际参数'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='检查重建任务表';

-- ----------------------------
-- Table structure for t_role_permission
-- ----------------------------
DROP TABLE IF EXISTS `t_role_permission`;
CREATE TABLE `t_role_permission` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `RoleId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '角色ID',
  `PermissionId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '权限ID',
  `PermissionCode` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '权限代码',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleId` (`RoleId`,`PermissionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色权限表';

-- ----------------------------
-- Table structure for t_roles
-- ----------------------------
DROP TABLE IF EXISTS `t_roles`;
CREATE TABLE `t_roles` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '名称',
  `Description` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '描述',
  `Level` smallint NOT NULL COMMENT 'Normal, Service, Admin',
  `IsFactory` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否出厂角色，出厂角色不可编辑和删除',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '标记删除',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='角色表';

-- ----------------------------
-- Table structure for t_scan_task
-- ----------------------------
DROP TABLE IF EXISTS `t_scan_task`;
CREATE TABLE `t_scan_task` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '主键Id',
  `InternalStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查id',
  `InternalPatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人ID',
  `FrameOfReferenceUid` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'FOR id',
  `MeasurementId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '当前scantask所属measurement组',
  `ScanId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '关联协议扫描区域表ID',
  `BodyPart` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '扫描区域',
  `ScanOption` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT '0' COMMENT '扫描类型',
  `ScanStartDate` datetime DEFAULT NULL COMMENT '扫描开始时间',
  `ScanEndDate` datetime DEFAULT NULL COMMENT '扫描结束时间',
  `IsDeleted` bit(1) DEFAULT b'0' COMMENT '是否删除',
  `Description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '描述',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人',
  `ScanTimeLength` int DEFAULT '20' COMMENT '扫描时间长度',
  `ProgressStep` int DEFAULT '0' COMMENT '进度条进度值',
  `StartSound` bit(1) DEFAULT b'0' COMMENT '是否播放前语音',
  `StartSoundTimeLength` int DEFAULT '0' COMMENT '前语音文件长度',
  `StartSoundFile` varchar(260) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '前语音文件路径',
  `StopSound` bit(1) DEFAULT b'0' COMMENT '是否播放后语音',
  `StopSoundTimeLength` int DEFAULT '0' COMMENT '后语音文件长度',
  `StopSoundFile` varchar(260) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '后语音文件路径',
  `TopoScanId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '定位像扫描任务ID',
  `BodySize` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '成人/儿童',
  `ExportStatus` int DEFAULT NULL COMMENT '导出状态（值：0待导出，1已导出）',
  `DeleteStatus` int DEFAULT NULL COMMENT '删除状态（值：0待删除，1已删除）',
  `Path` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'Raw数据磁盘路径',
  `IsLinkScan` bit(1) DEFAULT b'0' COMMENT '是否连扫',
  `IsInject` bit(1) DEFAULT b'0' COMMENT '是否注射',
  `IssuingParameters` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '下发参数',
  `ActuralParameters` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '实际参数',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='检查扫描任务表';

-- ----------------------------
-- Table structure for t_series
-- ----------------------------
DROP TABLE IF EXISTS `t_series`;
CREATE TABLE `t_series` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'StudyID',
  `SeriesInstanceUID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '唯一序列ID',
  `FrameOfReferenceUID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '唯一FOR ID',
  `ScanId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '扫描任务ID',
  `ReconId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '重建任务Id',
  `BodyPart` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查部位',
  `Modality` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '设备',
  `SeriesType` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '类型包括Doesreport  、Rawdata、SR、snapshot、imageseries、ecg',
  `ImageType` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '图片类型： 轴扫 螺旋 ',
  `SeriesNumber` int DEFAULT NULL COMMENT '序列号',
  `SeriesDescription` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '序列描述',
  `WindowType` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'Window类型',
  `WindowWidth` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '窗宽',
  `WindowLevel` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '窗位',
  `ReconStartDate` datetime DEFAULT NULL COMMENT '重建开始时间',
  `ReconEndDate` datetime DEFAULT NULL COMMENT '重建结束时间',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否删除',
  `PatientPosition` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人摆位',
  `ProtocolName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '协议名',
  `ImageCount` int DEFAULT NULL COMMENT '图像张数',
  `ReportPath` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '报告路径',
  `ArchiveStatus` int NOT NULL DEFAULT '0' COMMENT '归档状态',
  `PrintStatus` int DEFAULT NULL COMMENT '打印状态（值：0待打印，1打印中，2打印完成，3打印失败）',
  `IsProtected` bit(1) NOT NULL DEFAULT b'0' COMMENT '锁定状态',
  `CorrectStatus` int DEFAULT NULL COMMENT '校正状态',
  `SeriesPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '图像存储目录路径',
  PRIMARY KEY (`Id`),
  KEY `Index_StudyID` (`InternalStudyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='检查序列表';

-- ----------------------------
-- Table structure for t_study
-- ----------------------------
DROP TABLE IF EXISTS `t_study`;
CREATE TABLE `t_study` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalPatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT '' COMMENT 'Patient表中的ID',
  `StudyInstanceUID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Age` int DEFAULT NULL COMMENT '年龄',
  `AgeType` int DEFAULT NULL COMMENT '年龄单位：1:年;2:月;3:周;4:日',
  `PatientType` int DEFAULT NULL COMMENT '病人类型，1:急诊病人，2：预登记病人，3本地登记病人',
  `RequestProcedure` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '请求程序',
  `BodyPart` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查部位',
  `AccessionNo` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '登记号',
  `RegistrationDate` datetime DEFAULT NULL COMMENT '登记日期',
  `StudyTime` datetime DEFAULT NULL COMMENT '检查时间',  
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录创建时间', 
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人', 
  `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间',
  `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人',  
  `ExamStartTime` datetime DEFAULT NULL COMMENT '检查日期',
  `ExamEndTime` datetime DEFAULT NULL COMMENT '检查时间',
  `DeviceId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查设备编号',
  `Technician` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查技师',
  `LoginId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '登录人员ID',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '存储状态 ',
  `StudyStatus` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'Study检查状态',
  `PatientSize` double DEFAULT NULL COMMENT '身高',
  `PatientWeight` double DEFAULT NULL COMMENT '体重',
  `ReferringPhysicianName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '咨询医生',
  `Ward` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病房',
  `FieldStrenght` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病床',
  `AdmittingDiagnosisDescription` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '入院诊断',
  `MedicalAlerts` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '医疗警报',
  `Allergies` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '过敏',
  `SmokingStatus` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '吸烟',
  `PregnancyStatus` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '怀孕状态',
  `StudyDescription` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '检查描述',
  `PatientResidency` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人居住地址',
  `PatientAddress` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人地址',
  `BodyParts` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查的部位. 所有的检查部位 全部显示在一起',
  `PerformingPhysician` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ArchiveStatus` int NOT NULL DEFAULT '0' COMMENT '归档状态',
  `StudyPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查路径',
  `StudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查ID',
  `PrintStatus` int DEFAULT NULL COMMENT '打印状态（值：0待打印，1打印中，2已部分打印，3打印完成，打印失败）',
  `CorrectStatus` int DEFAULT NULL COMMENT '校正状态(值：待校正，已校正)',
  `IsProtected` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否锁定',
  `OriginStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '和上一个被校正的study关联',
  `MedicalHistory` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '病史',
  `InstitutionName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '团体/机构名称',
  `InstitutionAddress` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '团体/机构地址',
  `Comments` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '检查备注',
  `Protocol` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '检查协议',
  `PrintConfigPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '打印配置文件路径',
  `IsLocalModified` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否本地记录已修改',
  PRIMARY KEY (`Id`),
  KEY `Index_PatentID` (`InternalPatientId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='患者检查表';

-- ----------------------------
-- Table structure for t_user_role
-- ----------------------------
DROP TABLE IF EXISTS `t_user_role`;
CREATE TABLE `t_user_role` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `UserId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '用户ID',
  `UserAccount` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '用户登录账号',
  `RoleId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '角色ID',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserId` (`UserId`,`RoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户角色表';

-- ----------------------------
-- Table structure for t_users
-- ----------------------------
DROP TABLE IF EXISTS `t_users`;
CREATE TABLE `t_users` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Account` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '账号',
  `Password` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '密码',
  `FirstName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '姓名',
  `LastName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '',
  `Sex` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '性别',
  `Comments` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '备注',
  `IsFactory` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否系统默认账号',
  `IsLocked` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否锁定，锁定后不能登录',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '标记删除',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `WrongPassLoginTimes` int NOT NULL DEFAULT '0' COMMENT '当前登录错误重试累积次数',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Account` (`Account`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='用户表';

-- ----------------------------
-- Table structure for t_voices
-- ----------------------------
DROP TABLE IF EXISTS `t_voices`;
CREATE TABLE `t_voices` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalId` smallint NOT NULL DEFAULT '0' COMMENT '语音ID（数值型）',
  `Name` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '语音名称',
  `Description` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '语音描述',
  `BodyPart` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '检查部位',
  `FilePath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '语音文件路径',
  `IsFront` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否是前置语音',
  `VoiceLength` tinyint NOT NULL DEFAULT '0' COMMENT '语音时长',
  `RealVoiceLength` decimal(10,2) NOT NULL DEFAULT '0' COMMENT '真实语音时长',
  `Language` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '语音语言类型',
  `IsFactory` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否是出场设置语音',
  `IsDefault` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否是缺省语音',
  `IsValid` bit(1) NOT NULL DEFAULT b'1' COMMENT '是否是可用语音',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '语音创建时间',
  PRIMARY KEY (`Id`),
  KEY `InternalId` (`InternalId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='检查前后语音表';

-- ----------------------------
-- Table structure for t_patient_correction
-- ----------------------------
DROP TABLE IF EXISTS `t_patient_correction`;
CREATE TABLE `t_patient_correction` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalPatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '病人ID',
  `PatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL COMMENT '病人ID',
  `PatientName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人姓名',
  `PatientSex` int NOT NULL DEFAULT '0' COMMENT '性别',
  `PatientBirthDate` datetime DEFAULT NULL COMMENT '出生日期',
  `Editor` varchar(50) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '编辑人',
  `Creator` varchar(64) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '记录创建时间',
  `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间',
  `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人', 
  `PatientComments` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人注释',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='患者信息校正表';

-- ----------------------------
-- Table structure for t_raw_data
-- ----------------------------
DROP TABLE IF EXISTS `t_raw_data`;
CREATE TABLE `t_raw_data` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'StudyID',
  `FrameOfReferenceUID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '唯一FOR ID',
  `ScanId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '扫描任务ID',
  `ScanName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '扫描任务名称',
  `BodyPart` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查部位',
  `ProtocolName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '协议名',
  `PatientPosition` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人摆位',
  `ScanModel` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '扫描任务参数',
  `ScanEndTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '扫描结束时间',
  `Path` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '生数据存储目录路径',
  `IsExported` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否导出',
  `IsDeleted` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否删除',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`),
  KEY `Index_StudyID` (`InternalStudyId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='检查序列生数据表';

-- ----------------------------
-- Table structure for t_login_history
-- ----------------------------
DROP TABLE IF EXISTS `t_login_history`;
CREATE TABLE `t_login_history` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `Account` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '账号',
  `EncryptPassword` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '对称加密后密码',
  `Behavior` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '登录或登出',
  `Comments` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '备注',
  `FailReason` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '失败原因',
  `IsSuccess` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否登录成功',
  `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL DEFAULT '' COMMENT '创建人',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='登录历史记录表';

-- ----------------------------
-- Table structure for t_study_correction
-- ----------------------------
DROP TABLE IF EXISTS `t_study_correction`;
CREATE TABLE `t_study_correction` (
  `Id` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `InternalStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT '' COMMENT 'Study表中的ID',
  `InternalPatientId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT '' COMMENT 'Patient表中的ID',
  `StudyInstanceUID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `Age` int DEFAULT NULL COMMENT '年龄',
  `AgeType` int DEFAULT NULL COMMENT '年龄单位：1:年;2:月;3:周;4:日',
  `PatientType` int DEFAULT NULL COMMENT '病人类型，1:急诊病人，2：预登记病人，3本地登记病人',
  `RequestProcedure` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '请求程序',
  `BodyPart` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查部位',
  `AccessionNo` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '登记号',
  `RegistrationDate` datetime DEFAULT NULL COMMENT '登记日期',
  `StudyTime` datetime DEFAULT NULL COMMENT '检查时间',
  `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建日期',
  `Creator` varchar(64) COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人',
  `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间',
  `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人', 
  `ExamStartTime` datetime DEFAULT NULL COMMENT '检查日期',
  `ExamEndTime` datetime DEFAULT NULL COMMENT '检查时间',
  `DeviceId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查设备编号',
  `Technician` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查技师',
  `LoginId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '登录人员ID',
  `StudyStatus` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'Study检查状态',
  `PatientSize` double DEFAULT NULL COMMENT '身高',
  `PatientWeight` double DEFAULT NULL COMMENT '体重',
  `ReferringPhysicianName` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '咨询医生',
  `Ward` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病房',
  `FieldStrenght` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病床',
  `AdmittingDiagnosisDescription` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '入院诊断',
  `MedicalAlerts` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '医疗警报',
  `Allergies` varchar(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '过敏',
  `SmokingStatus` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '吸烟',
  `PregnancyStatus` varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '怀孕状态',
  `StudyDescription` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '检查描述',
  `PatientResidency` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人居住地址',
  `PatientAddress` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '病人地址',
  `BodyParts` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查的部位. 所有的检查部位 全部显示在一起',
  `PerformingPhysician` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL,
  `ArchiveStatus` int NOT NULL DEFAULT '0' COMMENT '归档状态',
  `StudyPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查路径',
  `StudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查ID',
  `PrintStatus` int DEFAULT NULL COMMENT '打印状态（值：0待打印，1打印中，2已部分打印，3打印完成，打印失败）',
  `CorrectStatus` int DEFAULT NULL COMMENT '校正状态(值：待校正，已校正)',
  `IsProtected` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否锁定',
  `OriginStudyId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '和上一个被校正的study关联',
  `MedicalHistory` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '病史',
  `InstitutionName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '团体/机构名称',
  `InstitutionAddress` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '团体/机构地址',
  `Comments` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '检查备注',
  `Protocol` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci COMMENT '检查协议',
  `PrintConfigPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '打印配置文件路径',
  `Editor` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '编辑人',
  `IsLocalModified` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否本地记录已修改',  
  PRIMARY KEY (`Id`),
  KEY `Index_PatentID` (`InternalPatientId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci COMMENT='患者检查校正表';
