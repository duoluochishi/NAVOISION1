//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="VStudy.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

namespace NV.CT.PatientManagement.ViewModel;

public class FilmPlayViewModel : BaseViewModel
{
    private int fps;
    private int playStatus;
    private string btnPlayIcon = string.Empty;
    private bool isStop = true;

    public FilmPlayViewModel()
    {
        InstanceViewModel = Global.Instance.ServiceProvider.GetRequiredService<InstanceViewModel>();
        FPS = 10;
        playStatus = 1;
        BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Suspend_0.png";
        Commands.Add("PreviousCommand", new DelegateCommand(Previous));
        Commands.Add("NextCommand", new DelegateCommand(Next));
        Commands.Add("PlayOrPauseCommand", new DelegateCommand(PlayOrPause));
        Commands.Add("SpeedUpCommand", new DelegateCommand(SpeedUp));
        Commands.Add("SpeedDownCommand", new DelegateCommand(SpeedDown));
        InstanceViewModel.TomoImageViewerWrapper.OnCineFinished += TomoImageViewerWrapper_OnCineFinished;
        PlayOrPause();
    }

    public int FPS
    {
        get => fps;
        set => SetProperty(ref fps, value);
    }
    public string BtnPlayIcon
    {
        get => btnPlayIcon;
        set => SetProperty(ref btnPlayIcon, value);
    }
    public InstanceViewModel InstanceViewModel { get; set; }


    private void TomoImageViewerWrapper_OnCineFinished(object? sender, int e)
    {
        isStop = true;
        playStatus = 1;
        BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Play_0.png";
    }
    private void SpeedDown()
    {
        if (FPS == 1)
        {
            return;
        }
        FPS--;
        InstanceViewModel.TomoImageViewerWrapper.AdjustFrameRateCine(1100 / FPS);
    }

    private void SpeedUp()
    {
        if (FPS == 100)
        {
            return;
        }
        FPS++;
        InstanceViewModel.TomoImageViewerWrapper.AdjustFrameRateCine(1100 / FPS);
    }

    private void PlayOrPause()
    {
        isStop = false;
        switch (playStatus)
        {
            case (int)PlayStatus.Start:
                InstanceViewModel.TomoImageViewerWrapper.SetCineParam(fps);
                InstanceViewModel.TomoImageViewerWrapper.CineStart();
                playStatus = 2;
                BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Suspend_0.png";
                break;
            case (int)PlayStatus.Pause:
                InstanceViewModel.TomoImageViewerWrapper.CinePause();
                playStatus = 3;
                BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Play_0.png";
                break;
            case (int)PlayStatus.Resume:
                InstanceViewModel.TomoImageViewerWrapper.CinePauseResume();
                playStatus = 2;
                BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Suspend_0.png";
                break;
        }
    }

    private void Next()
    {
        if (!isStop)
        {
            playStatus = 3;
        }
        BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Play_0.png";
        InstanceViewModel.TomoImageViewerWrapper.MoveToNextSlice();
        InstanceViewModel.TomoImageViewerWrapper.CineForward();
    }

    private void Previous()
    {
        if (!isStop)
        {
            playStatus = 3;
        }
        BtnPlayIcon = "pack://application:,,,/NV.CT.UI.Controls;component/Icons/Play_0.png";
        InstanceViewModel.TomoImageViewerWrapper.MoveToPriorSlice();
        InstanceViewModel.TomoImageViewerWrapper.CineForward();
    }
}