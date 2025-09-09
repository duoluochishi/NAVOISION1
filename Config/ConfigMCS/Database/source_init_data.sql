use db_mcs;

INSERT INTO `t_voices` VALUES (uuid(), '1', 'en_US_Std_Breathe_In', 'en_US_Std_Breathe_In', '', 'Factory\\1.wav', 1, '3','2.94', 'EN', 1, 0, 1, '', current_timestamp());
INSERT INTO `t_voices` VALUES (uuid(), '2', 'en_US_Std_Breathe_Out', 'en_US_Std_Breathe_Out', '', 'Factory\\2.wav', 0, '2','1.67', 'EN', 1, 0, 1, '', current_timestamp());
INSERT INTO `t_voices` VALUES (uuid(), '3', 'Please_breathe', 'Please_breathe', '', 'Factory\\3.wav', 0, '2','1.83', 'CN', 1, 0, 1, '', current_timestamp());
INSERT INTO `t_voices` VALUES (uuid(), '4', 'Take_a_deep_breath_in_and_hold_it', 'Take_a_deep_breath_in_and_hold_it', '', 'Factory\\4.wav', 1, '3','2.94', 'CN', 0, 0, 1, '', current_timestamp());
INSERT INTO `t_voices` VALUES (uuid(), '5', 'zh_CH_Std_Breathe_In', 'zh_CH_Std_Breathe_In', '', 'Factory\\5.wav', 1, '3','2.97', 'CN', 1, 1, 1, '', current_timestamp());
INSERT INTO `t_voices` VALUES (uuid(), '6', 'zh_CH_Std_Breathe_Out', 'zh_CH_Std_Breathe_Out', '', 'Factory\\6.wav', 0, '2','1.64', 'CN', 1, 1, 1, '', current_timestamp());

INSERT INTO `t_permissions` VALUES (uuid(), 'UserManagement', 'UserManagement', 'User management', 'SecuritySetting', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ImageTextConfig', 'ImageTextConfig', 'Image text config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ViewAuditLog', 'ViewAuditLog', 'View audit log', 'SecuritySetting', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DataDeletion', 'DataDeletion', 'Data delete', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'BackupRestore', 'BackupRestore', 'Backup restore', 'DeviceMaintain', '1', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ArchiveAuditLog', 'ArchiveAuditLog', 'Archive audit log', 'SecuritySetting', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'MainAEConfig', 'MainAEConfig', 'Main AE config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ModeTypeConfig', 'ModeTypeConfig', 'Mode type config', 'SystemSetting', '1', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DeleteAuditLog', 'DeleteAuditLog', 'Delete audit log', 'SecuritySetting', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ProtocolManagement', 'ProtocolManagement', 'Protocol management', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DicomNodeConfig', 'DicomNodeConfig', 'Dicom node config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'CommonSetting', 'CommonSetting', 'Common setting', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'HospitalSetting', 'HospitalSetting', 'Hospital setting', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'SystemConfig', 'SystemConfig', 'System config', 'SystemSetting', '1', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DataProtection', 'DataProtection', 'Data protect', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'AutoArchiveConfig', 'AutoArchiveConfig', 'Auto archive config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ServiceMaintain', 'ServiceMaintain', 'Service maintain', 'DeviceMaintain', '1', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DailyCalibration', 'DailyCalibration', 'Daily calibration', 'DeviceMaintain', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'Print', 'Print', 'Print', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'CreateNewExam', 'CreateNewExam', 'Create new exam', 'PatientRegistration', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'LogManagement', 'LogManagement', 'Log management', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ImportRaw', 'ImportRaw', 'Import raw', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DeviceTest', 'DeviceTest', 'Device test', 'DeviceMaintain', '1', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'AutoDeleteConfig', 'AutoDeleteConfig', 'Auto delete config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'PatientRegistConfig', 'PatientRegistConfig', 'Patient regist config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'FilmConfig', 'FilmConfig', 'Film config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'Export', 'Export', 'Export', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'AutoPrintConfig', 'AutoPrintConfig', 'Auto print config', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'EmergencyExam', 'EmergencyExam', 'Emergency exam', 'PatientRegistration', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'QueryRetrieve', 'QueryRetrieve', 'Query retrieve', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DevDebug', 'DevDebug', 'Dev debug', 'DeviceMaintain', '2', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ExportRaw', 'ExportRaw', 'Export raw', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ImageViewer', 'ImageViewer', 'Image viewer', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'Import', 'Import', 'Import', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'CorrectInfo', 'CorrectInfo', 'Correct info', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'Exam', 'Exam', 'Exam', 'Exam', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'WindowingPreset', 'WindowingPreset', 'Windowing preset', 'ClinicalTools', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'ExceedAlertScan', 'ExceedAlertScan', 'Exceed Alert Scan', 'Exam', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'OfflineRecon', 'OfflineRecon', 'Offline recon', 'Exam', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'Archive', 'Archive', 'Archive', 'PatientManagement', '0', '\0', '', current_timestamp());
INSERT INTO `t_permissions` VALUES (uuid(), 'DeviceMaintain', 'DeviceMaintain', 'Device maintain', 'DeviceMaintain', '1', '\0', '', current_timestamp());

INSERT INTO `t_roles` VALUES (uuid(), 'ServiceEngineer', 'Service engineer', '1', '', '\0', '', current_timestamp());
INSERT INTO `t_roles` VALUES (uuid(), 'Administrator', 'Administrator', '2', '', '\0', '', current_timestamp());
INSERT INTO `t_roles` VALUES (uuid(), 'Operator', 'Operator', '0', '', '\0', '', current_timestamp());
INSERT INTO `t_roles` VALUES (uuid(), 'Engineer', 'Engineer', '0', '', '\0', '', current_timestamp());
INSERT INTO `t_roles` VALUES (uuid(), 'Senior', 'Senior', '0', '', '\0', '', current_timestamp());
INSERT INTO `t_roles` VALUES (uuid(), 'DeviceManager', 'Device manager', '0', '', '\0', '', current_timestamp());
INSERT INTO `t_roles` VALUES (uuid(), 'Emergency', 'Emergency user', '0', '', '\0', '', current_timestamp());

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Senior', 'Engineer', 'DeviceManager')
AND p.Code in ('UserManagement', 'ViewAuditLog', 'ArchiveAuditLog', 'DeleteAuditLog');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Operator', 'Engineer', 'Senior', 'DeviceManager', 'Emergency')
AND p.Code in ('CreateNewExam', 'EmergencyExam');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Operator', 'Engineer', 'Senior', 'DeviceManager', 'Emergency')
AND p.Code in ('Exam', 'OfflineRecon');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Engineer', 'Senior', 'DeviceManager', 'Emergency')
AND p.Code = 'ExceedAlertScan';

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Operator', 'Engineer', 'Senior', 'DeviceManager', 'Emergency')
AND p.Code in ('Archive', 'Export', 'ImageViewer');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Operator', 'Engineer', 'Senior', 'DeviceManager')
AND p.Code in ('Import', 'QueryRetrieve', 'CorrectInfo', 'DataProtection', 'DataDeletion', 'Print');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Engineer', 'Senior')
AND p.Code in ('ExportRaw', 'ImportRaw');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Operator', 'Engineer', 'Senior', 'DeviceManager')
AND p.Code in ('ImageTextConfig', 'PatientRegistConfig', 'WindowingPreset', 'CommonSetting');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Engineer', 'Senior', 'DeviceManager')
AND p.Code in ('FilmConfig', 'AutoArchiveConfig', 'AutoPrintConfig', 'AutoDeleleConfig', 'HospitalSetting', 'MainAEConfig', 'DicomNodeConfig', 'LogManagement', 'ProtocolManagement');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Engineer', 'Senior', 'DeviceManager')
AND p.Code = 'DailyCalibration';

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Engineer', 'Senior', 'DeviceManager')
AND p.Code = 'DeviceMaintain';

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Engineer', 'Senior')
AND p.Code in ('BackupRestore', 'ServiceMaintain');

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('ServiceEngineer', 'Administrator', 'Senior')
AND p.Code = 'DeviceTest';

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name = 'Administrator'
AND p.Code = 'DevDebug';

INSERT INTO `t_role_permission`(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime)
SELECT uuid(), r.ID, p.ID, p.Code, '', CURRENT_TIMESTAMP()
FROM t_roles r, t_permissions p
WHERE r.Name in ('Administrator', 'Senior')
AND p.Code in ('SystemConfig', 'ModeTypeConfig');

-- Password:123456
INSERT INTO `t_users` VALUES (uuid(), 'NanoUser', '7C81D8BACFCE8F04836F6E12239ADEA1', 'Nano', 'Nano', 'Male', 'Default Users', 1, 0, 0, '', current_timestamp(),0);
INSERT INTO `t_users` VALUES (uuid(), 'NanoEngineer', '7C81D8BACFCE8F04836F6E12239ADEA1', 'Engineer', 'Engineer', 'Male', 'Default Users', 1, 0, 0, '', current_timestamp(),0);
INSERT INTO `t_users` VALUES (uuid(), 'NanoService', '7C81D8BACFCE8F04836F6E12239ADEA1', 'Service', 'Service', 'Male', 'Default Users', 1, 0, 0, '', current_timestamp(),0);
INSERT INTO `t_users` VALUES (uuid(), 'NanoSenior', '7C81D8BACFCE8F04836F6E12239ADEA1', 'Senior', 'Senior', 'Male', 'Default Users', 1, 0, 0, '', current_timestamp(),0);
INSERT INTO `t_users` VALUES (uuid(), 'NanoAdmin', '7C81D8BACFCE8F04836F6E12239ADEA1', 'Administrator', 'Admin', 'Male', 'Default Users', 1, 0, 0, '', current_timestamp(),0);
INSERT INTO `t_users` VALUES (uuid(), 'NanoDeviceManager', '7C81D8BACFCE8F04836F6E12239ADEA1', 'DeviceManager', 'Device', 'Male', 'Default Users', 1, 0, 0, '', current_timestamp(),0);

INSERT INTO `t_user_role`(Id,UserId,UserAccount,RoleId,Creator,CreateTime)
SELECT uuid(), u.Id, u.Account, r.Id, '', CURRENT_TIMESTAMP()
FROM t_users u, t_roles r
WHERE u.Account = 'NanoUser' and r.Name = 'Operator';

INSERT INTO `t_user_role`(Id,UserId,UserAccount,RoleId,Creator,CreateTime)
SELECT uuid(), u.Id, u.Account, r.Id, '', CURRENT_TIMESTAMP()
FROM t_users u, t_roles r
WHERE u.Account = 'NanoEngineer' and r.Name = 'Engineer';

INSERT INTO `t_user_role`(Id,UserId,UserAccount,RoleId,Creator,CreateTime)
SELECT uuid(), u.Id, u.Account, r.Id, '', CURRENT_TIMESTAMP()
FROM t_users u, t_roles r
WHERE u.Account = 'NanoService' and r.Name = 'ServiceEngineer';

INSERT INTO `t_user_role`(Id,UserId,UserAccount,RoleId,Creator,CreateTime)
SELECT uuid(), u.Id, u.Account, r.Id, '', CURRENT_TIMESTAMP()
FROM t_users u, t_roles r
WHERE u.Account = 'NanoSenior' and r.Name = 'Senior';

INSERT INTO `t_user_role`(Id,UserId,UserAccount,RoleId,Creator,CreateTime)
SELECT uuid(), u.Id, u.Account, r.Id, '', CURRENT_TIMESTAMP()
FROM t_users u, t_roles r
WHERE u.Account = 'NanoAdmin' and r.Name = 'Administrator';

INSERT INTO `t_user_role`(Id,UserId,UserAccount,RoleId,Creator,CreateTime)
SELECT uuid(), u.Id, u.Account, r.Id, '', CURRENT_TIMESTAMP()
FROM t_users u, t_roles r
WHERE u.Account = 'NanoDeviceManager' and r.Name = 'DeviceManager';