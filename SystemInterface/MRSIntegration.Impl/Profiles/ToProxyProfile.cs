//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.SystemInterface.MRSIntegration.Impl.Profiles;

public class ToProxyProfile : Profile
{
    public ToProxyProfile()
    {
        CreateMap<PatientSex, CTS.Enums.Gender>().ConvertUsingEnumMapping(opt => opt.MapByValue()).ReverseMap();
        CreateMap<CTS.Enums.BodyPart, BodyPart>().ConvertUsingEnumMapping(opt => opt.MapValue(CTS.Enums.BodyPart.Spine, BodyPart.SPINE)
            .MapValue(CTS.Enums.BodyPart.Shoulder, BodyPart.SHOULDER)
            .MapValue(CTS.Enums.BodyPart.Pelvis, BodyPart.PELVIS)
            .MapValue(CTS.Enums.BodyPart.Neck, BodyPart.NECK)
            .MapValue(CTS.Enums.BodyPart.Head, BodyPart.HEAD)
            .MapValue(CTS.Enums.BodyPart.Chest, BodyPart.CHEST)
            .MapValue(CTS.Enums.BodyPart.Lung, BodyPart.LUNG)
            .MapValue(CTS.Enums.BodyPart.Breast, BodyPart.BREAST)
            .MapValue(CTS.Enums.BodyPart.Abdomen, BodyPart.ABDOMEN)
            .MapValue(CTS.Enums.BodyPart.Leg, BodyPart.LEG)
            .MapValue(CTS.Enums.BodyPart.Arm, BodyPart.ARM)
            .MapValue(CTS.Enums.BodyPart.Iac, BodyPart.IAC)
            .MapValue(CTS.Enums.BodyPart.Eye, BodyPart.EYE)
            .MapValue(CTS.Enums.BodyPart.Nose, BodyPart.NOSE)
            .MapValue(CTS.Enums.BodyPart.BHead, BodyPart.HEAD)
            .MapValue(CTS.Enums.BodyPart.BNeck, BodyPart.NECK)
            .MapValue(CTS.Enums.BodyPart.BChest, BodyPart.CHEST)
            .MapValue(CTS.Enums.BodyPart.BBreast, BodyPart.BREAST)
            .MapValue(CTS.Enums.BodyPart.BAbdomen, BodyPart.ABDOMEN)
            .MapValue(CTS.Enums.BodyPart.BArm, BodyPart.ARM)
            .MapValue(CTS.Enums.BodyPart.BLeg, BodyPart.LEG));
    }
}