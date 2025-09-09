using System.Windows.Media;

namespace NV.CT.RGT.View;

public partial class CountDown
{
    public CountDown()
    {
        InitializeComponent();

        lblValue.Content = "";
    }

    public bool Clockwise { get; set; }
    public double TotalSeconds { get; set; }

    public double CurrentValue
    {
        set => SetValue(value);
    }

    public double CurrentSecond { get; set; }
    public double EyeStickSecond { get; set; }

    public void UpdateStartLabel()
    {
        if (Clockwise)
        {
            lblValue.Content = "0 s";
        }
        else
        {
            lblValue.Content = $"{TotalSeconds} s";
        }
    }

    public void UpdateEndLabel()
    {
        if (Clockwise)
        {
            lblValue.Content = $"{TotalSeconds} s";
        }
        else
        {
            lblValue.Content = "0 s";
        }
    }

    /// <summary>
    /// 设置百分百，输入小数，自动乘100
    /// </summary>
    /// <param name="percentValue"></param>
    public void SetValue(double percentValue)
    {
        /*****************************************
          方形矩阵边长为34，半长为17
          环形半径为14，所以距离边框3个像素
          环形描边3个像素
        ******************************************/
        double angel = percentValue * 360; //角度

        double radius = 14; //环形半径

        //起始点
        double leftStart = 17;
        double topStart = 3;

        //结束点
        double endLeft;
        double endTop;

        //Log(CurrentSecond.ToString() + " , " + percentValue.ToString());

        //数字显示
        //lblValue.Content = (percentValue * 100).ToString("0") + "%";
        if (CurrentSecond < 0)
        {
            lblValue.Content = $"0 s";
        }
        else
        {
            if (Math.Abs(CurrentSecond - EyeStickSecond) >= 1)
            {
                //lblValue.Content = $"{Math.Floor(CurrentSecond)} s";
                lblValue.Content = $"{CurrentSecond:f0} s";
                //lblValue.Content = $"{(int)CurrentSecond} s";

                if (Clockwise)
                {
                    EyeStickSecond += 1;
                }
                else
                {
                    EyeStickSecond -= 1;
                }
            }
        }

        /***********************************************
        * 整个环形进度条使用Path来绘制，采用三角函数来计算
        * 环形根据角度来分别绘制，以90度划分，方便计算比例
        ***********************************************/

        bool isLagreCircle = false; //是否优势弧，即大于180度的弧形

        //小于90度
        if (angel <= 90)
        {
            /*****************
                      *
                      *   *
                      * * ra
               * * * * * * * * *
                      *
                      *
                      *
            ******************/
            double ra = (90 - angel) * Math.PI / 180; //弧度
            endLeft = leftStart + Math.Cos(ra) * radius; //余弦横坐标
            endTop = topStart + radius - Math.Sin(ra) * radius; //正弦纵坐标

        }

        else if (angel <= 180)
        {
            /*****************
                      *
                      *  
                      * 
               * * * * * * * * *
                      * * ra
                      *  *
                      *   *
            ******************/

            double ra = (angel - 90) * Math.PI / 180; //弧度
            endLeft = leftStart + Math.Cos(ra) * radius; //余弦横坐标
            endTop = topStart + radius + Math.Sin(ra) * radius;//正弦纵坐标
        }

        else if (angel <= 270)
        {
            /*****************
                      *
                      *  
                      * 
               * * * * * * * * *
                    * *
                   *ra*
                  *   *
            ******************/
            isLagreCircle = true; //优势弧
            double ra = (angel - 180) * Math.PI / 180;
            endLeft = leftStart - Math.Sin(ra) * radius;
            endTop = topStart + radius + Math.Cos(ra) * radius;
        }

        else if (angel < 360)
        {
            /*****************
                  *   *
                   *  *  
                 ra * * 
               * * * * * * * * *
                      *
                      *
                      *
            ******************/
            isLagreCircle = true; //优势弧
            double ra = (angel - 270) * Math.PI / 180;
            endLeft = leftStart - Math.Cos(ra) * radius;
            endTop = topStart + radius - Math.Sin(ra) * radius;
        }
        else
        {
            isLagreCircle = true; //优势弧
            endLeft = leftStart - 0.001; //不与起点在同一点，避免重叠绘制出非环形
            endTop = topStart;
        }

        Point arcEndPt = new Point(endLeft, endTop); //结束点
        Size arcSize = new Size(radius, radius);
        SweepDirection direction = SweepDirection.Clockwise; //顺时针弧形
        //弧形
        ArcSegment arcsegment = new ArcSegment(arcEndPt, arcSize, 0, isLagreCircle, direction, true);

        //形状集合
        PathSegmentCollection pathsegmentCollection = new PathSegmentCollection();
        pathsegmentCollection.Add(arcsegment);

        //路径描述
        PathFigure pathFigure = new PathFigure();
        pathFigure.StartPoint = new Point(leftStart, topStart); //起始地址
        pathFigure.Segments = pathsegmentCollection;

        //路径描述集合
        PathFigureCollection pathFigureCollection = new PathFigureCollection();
        pathFigureCollection.Add(pathFigure);

        //复杂形状
        PathGeometry pathGeometry = new PathGeometry();
        pathGeometry.Figures = pathFigureCollection;

        //Data赋值
        myCycleProcessBar1.Data = pathGeometry;
        //达到100%则闭合整个
        if (angel == 360)
            myCycleProcessBar1.Data = Geometry.Parse(myCycleProcessBar1.Data + " z");
    }
}