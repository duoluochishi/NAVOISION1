using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Models.MotionControl.Table;
using System;

namespace NV.CT.Service.HardwareTest.Categories
{
    public partial class TableBaseCategory : ObservableObject
    {
        /// <summary>
        /// 床方向
        /// </summary>
        [ObservableProperty]
        private TableMoveDirection direction = TableMoveDirection.Horizontal;
        /// <summary>
        /// 床水平位置（单位：mm, 精度0.001mm,即1um）
        /// </summary>
        [ObservableProperty]
        private float horizontalPosition = -1000;
        /// <summary>
        /// 床水平速度（单位：mm/s, 精度0.001mm/s,即1um/s）
        /// </summary>
        [ObservableProperty]
        private float horizontalVelocity = 100;
        /// <summary>
        /// 床垂直位置（单位：mm, 精度0.001mm,即1um）
        /// </summary>
        [ObservableProperty]
        private float verticalPosition = 730;
        /// <summary>
        /// 床垂直速度（单位：mm/s, 精度0.001mm/s,即1um/s）
        /// </summary>
        [ObservableProperty]
        private float verticalVelocity = 100;
        /// <summary>
        /// X轴位置（单位：mm, 精度0.001mm,即1um）
        /// </summary>
        [ObservableProperty]
        private float axisXPosition = 100;
        /// <summary>
        /// X轴速度（单位：mm/s, 精度0.001mm,即1um/s）
        /// </summary>
        [ObservableProperty]
        private float axisXVelocity = 100;

        /// <summary>
        /// 水平目标位置变化事件
        /// </summary>
        public event EventHandler<float>? HorizontalPositionChanged;
        /// <summary>
        /// 垂直目标位置变化事件
        /// </summary>
        public event EventHandler<float>? VerticalPositionChanged;
        /// <summary>
        /// X轴目标位置变化事件
        /// </summary>
        public event EventHandler<float>? AxisXPositionChanged;

        partial void OnHorizontalPositionChanged(float oldValue, float newValue)
        {
            HorizontalPositionChanged?.Invoke(this, newValue);
        }

        partial void OnVerticalPositionChanged(float oldValue, float newValue)
        {
            VerticalPositionChanged?.Invoke(this, newValue);
        }

        partial void OnAxisXPositionChanged(float oldValue, float newValue)
        {
            AxisXPositionChanged?.Invoke(this, newValue);
        }

    }
}
