## Since the fllowing syntax is not supported for scenario 'ADD COLUMN IF NOT EXISTS' : ALTER TABLE `t_job_task` ADD COLUMN IF NOT EXISTS `CreateTime` ..., 
## we have to use an idempotency Stored Procedure instead
## ***** authored by an.hu@nanovision.com.cn on 2024-07-24  ***********

use db_mcs;
DELIMITER $$
DROP PROCEDURE IF EXISTS upgradeDBon20240723; $$
CREATE PROCEDURE upgradeDBon20240723()
BEGIN
  SET @schemaName = "db_mcs";
  
  ########## Upgrade for t_series  ##########  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
				  WHERE COLUMN_NAME = 'WindowType' AND TABLE_NAME = 't_series' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_series` ADD COLUMN `WindowType` VARCHAR(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT 'Window类型'; 
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
				  WHERE COLUMN_NAME = 'SeriesNumber' AND TABLE_NAME = 't_series' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_series` ADD COLUMN `SeriesNumber` int DEFAULT NULL COMMENT '序列号' AFTER ImageType; 
  END IF;
  
  ########## End for t_series  ##########
  
  ########## Upgrade for t_job_task  ##########
  IF EXISTS ( SELECT 1 FROM information_schema.COLUMNS
              WHERE COLUMN_NAME = 'CreatedDateTime' AND TABLE_NAME = 't_job_task' AND TABLE_SCHEMA = @schemaName ) 
  THEN
    ALTER TABLE `t_job_task` RENAME COLUMN CreatedDateTime TO CreateTime;
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
				  WHERE COLUMN_NAME = 'UpdateTime' AND TABLE_NAME = 't_job_task' AND TABLE_SCHEMA = @schemaName ) 
  THEN
    ALTER TABLE `t_job_task`
    ADD COLUMN `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间' AFTER CREATOR,
    ADD COLUMN `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人' AFTER CREATOR;
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
				  WHERE COLUMN_NAME = 'InternalPatientID' AND TABLE_NAME = 't_job_task' AND TABLE_SCHEMA = @schemaName ) 
  THEN
    ALTER TABLE `t_job_task`
    ADD COLUMN `InternalPatientID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '患者ID(内部)' AFTER WorkflowId,
    ADD COLUMN `InternalStudyID` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '检查ID(内部)' AFTER WorkflowId;
  END IF;
  
  
  ########## End for t_job_task  ##########
  
  ########## Upgrade for t_patient  ##########  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
                   WHERE COLUMN_NAME = 'UpdateTime' AND TABLE_NAME = 't_patient' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_patient`
	ADD COLUMN `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人' AFTER CreateTime,
	ADD COLUMN `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间' AFTER CreateTime,
	ADD COLUMN `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人' AFTER CreateTime;
  END IF;
  ########## End for t_patient  ##########
  
  ########## Upgrade for t_patient_correction  ##########  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
				  WHERE COLUMN_NAME = 'UpdateTime' AND TABLE_NAME = 't_patient_correction' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_patient_correction`
	ADD COLUMN `UpdateTime` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间' AFTER CreateTime,
	ADD COLUMN `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人' AFTER CreateTime;
  END IF;
  ########## End for t_patient_correction  ##########
  
  ########## Upgrade for t_recon_task  ##########  
  IF EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'Creater' AND TABLE_NAME = 't_recon_task' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_recon_task` RENAME COLUMN `Creater` TO `Creator`;
  END IF;
  ########## End for t_recon_task  ##########

  ########## Upgrade for t_recon_task  ##########  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'SeriesNumber' AND TABLE_NAME = 't_recon_task' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_recon_task`
	ADD COLUMN `SeriesNumber`  int UNSIGNED NULL DEFAULT 0 AFTER `IsRTD`;
  END IF;
  ########## End for t_recon_task  ##########
  
  ########## Upgrade for t_scan_task  ##########  
  IF EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'Creater' AND TABLE_NAME = 't_scan_task' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_scan_task` RENAME COLUMN `Creater` TO `Creator`;
  END IF;
  ########## End for t_scan_task  ##########

  ########## Upgrade for t_dose_check  ##########  
  IF EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE (COLUMN_NAME = 'FrameOfReferenceId' OR COLUMN_NAME = 'MeasurementId' OR COLUMN_NAME = 'ScanId') AND TABLE_NAME = 't_dose_check' AND TABLE_SCHEMA = @schemaName ) 
  THEN
		ALTER TABLE t_dose_check MODIFY COLUMN FrameOfReferenceId VARCHAR(64); 
		ALTER TABLE t_dose_check MODIFY COLUMN MeasurementId VARCHAR(64); 
		ALTER TABLE t_dose_check MODIFY COLUMN ScanId VARCHAR(64); 
  END IF;
  ########## End for t_dose_check  ##########
  
  ########## Upgrade for t_users  ##########  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'WrongPassLoginTimes' AND TABLE_NAME = 't_users' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_users`
	ADD COLUMN `WrongPassLoginTimes` int NOT NULL DEFAULT '0' COMMENT '错误密码重试累积次数';
  END IF;
  ########## End for t_users  ##########
  
  ########## Upgrade for t_study  ##########  
  IF EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'StudyDate' AND TABLE_NAME = 't_study' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_study` DROP COLUMN `StudyDate`;
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			      WHERE COLUMN_NAME = 'UpdateTime' AND TABLE_NAME = 't_study' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_study`
	ADD COLUMN `CreateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录创建时间' AFTER StudyTime,
	ADD COLUMN `Creator` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '创建人' AFTER StudyTime,
	ADD COLUMN `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间' AFTER StudyTime,
	ADD COLUMN `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人' AFTER StudyTime,
	ADD COLUMN `IsLocalModified` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否本地记录已修改';
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			      WHERE COLUMN_NAME = 'PrintConfigPath' AND TABLE_NAME = 't_study' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_study`
	ADD COLUMN `PrintConfigPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '打印配置文件路径' AFTER Protocol;
  END IF;  
  
  ########## End for t_study  ##########
  
  ########## Upgrade for t_study_correction  ##########  
  IF EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'StudyDate' AND TABLE_NAME = 't_study_correction' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_study_correction` DROP COLUMN `StudyDate`;
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			      WHERE COLUMN_NAME = 'UpdateTime' AND TABLE_NAME = 't_study_correction' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_study_correction`
	ADD COLUMN `UpdateTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '记录修改时间' AFTER StudyTime,
	ADD COLUMN `Updater` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '修改人' AFTER StudyTime,
	ADD COLUMN `IsLocalModified` bit(1) NOT NULL DEFAULT b'0' COMMENT '是否本地记录已修改';
  END IF;
  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			      WHERE COLUMN_NAME = 'PrintConfigPath' AND TABLE_NAME = 't_study_correction' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_study_correction`
    ADD COLUMN `PrintConfigPath` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '打印配置文件路径' AFTER Protocol;
  END IF;
  
  ########## End for t_study_correction  ##########
  
  ########## Upgrade for t_voices  ##########  
  IF NOT EXISTS ( SELECT 1 FROM information_schema.COLUMNS
			  WHERE COLUMN_NAME = 'RealVoiceLength' AND TABLE_NAME = 't_voices' AND TABLE_SCHEMA = @schemaName ) 
  THEN
	ALTER TABLE `t_voices` 
	ADD COLUMN `RealVoiceLength` decimal(10,2) NOT NULL DEFAULT '0' COMMENT '真实语音时长' AFTER VoiceLength;
  END IF;
  ########## End for t_voices  ##########
  
END $$
DELIMITER ;

CALL upgradeDBon20240723;
DROP PROCEDURE upgradeDBon20240723;
