
using MicroMvvm;
using MouseMovements.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Control = System.Windows.Forms.Control;
using Point = System.Windows.Point;

namespace MouseMovements.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
#region members
        private int width;
        private int height;
        private int currentPointerX;
        private int currentPointerY;
        private int currentPositionX;
        private int currentPositionY;
        private bool isBtnDisplayed;
        List<MousePoint> allMousePoints;
        Stopwatch stopwatch;
        MousePoint mousePoint;
        MousePath mousePath;
        private Visibility showMainUserControl;
        private Visibility enterVisibility;
        private string nameOfUser;
        private string filePath;
        private string folderPath;
        private bool nameInserted;
        private int countingFiles;
        private bool canExecutePlayingLast;
        private bool canExecuteStartRecord;
        private bool canExecuteStopRecord;
        private bool started;
        #endregion

        public MainWindowViewModel()
        {
            started = false;
            canExecuteStartRecord = true;
            canExecuteStopRecord = false;
            canExecutePlayingLast = false;
            countingFiles= 0;
            enterVisibility = Visibility.Visible;
            nameOfUser = "";
            showMainUserControl = Visibility.Collapsed;
            allMousePoints = new();
            isBtnDisplayed= false;
            isBtnDisplayed = true;
            stopwatch = new Stopwatch();
            nameInserted = false;
        }
     
        #region Properties
        public string NameOfUser
        {
            get
            {
                return nameOfUser;
            }
            set
            {
                nameOfUser = value;               
                OnPropertyChanged(nameof(NameOfUser));
            }
        }
        public Visibility EnterVisibility
        {
            get
            {
                return enterVisibility;
            }
            set
            {
                enterVisibility = value;
                OnPropertyChanged(nameof(EnterVisibility));
            }
        }
        public Visibility ShowMainUserControl
        {
            get
            {
                return showMainUserControl;
            }
            set
            {
                showMainUserControl = value;
                OnPropertyChanged(nameof(ShowMainUserControl));
            }
        }
        public int Width 
        { 
            get 
            {
                return width;
            }
            set
            {
                width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
        public int CurrentPointerX
        {
            get
            {
                return currentPointerX;
            }
            set
            {
                currentPointerX = value;
                OnPropertyChanged(nameof(CurrentPointerX));
            }
        }
        public int CurrentPointerY
        {
            get
            {
                return currentPointerY;
            }
            set
            {
                currentPointerY = value;
                OnPropertyChanged(nameof(CurrentPointerY));
            }
        }
        public int CurrentPositionX
        {
            get
            {
                return currentPositionX;
            }
            set
            {
                currentPositionX = value;
                OnPropertyChanged(nameof(CurrentPositionX));
            }
        }
        public int CurrentPositionY
        {
            get
            {
                return currentPositionY;
            }
            set
            {
                currentPositionY = value;
                OnPropertyChanged(nameof(CurrentPositionY));
            }
        }
        #endregion
       private void CheckCanExecutePlayingLast()
        {
            if (countingFiles != 0)
            {
                canExecutePlayingLast = true;
            }
            else
            {
                canExecutePlayingLast = false;
            }
        }
        public static Point MakeButton()
        {
            Random r = new Random();
            int width = r.Next(50, 100);
            int heigh = r.Next(15, 55);
            return new Point(width, heigh);
        }
        public Point GetCoordinates()
        {
            double x = System.Windows.SystemParameters.PrimaryScreenWidth;
            double y = System.Windows.SystemParameters.PrimaryScreenHeight;
            Random r = new Random();
            int randomX = r.Next(0, (int)x - Width);
            int randomY = r.Next(0, (int)y - Height);
            return new Point(randomX, randomY);
        }
        private void MovingMouseSimulate()
        {
            if (nameInserted)
            {
                Point mouseXY = GetMousePosition();
                CurrentPointerX = (int)mouseXY.X;
                CurrentPointerY = (int)mouseXY.Y;
                if (started)
                {
                    mousePoint.coordinate = mouseXY;
                    mousePoint.elapsedMS = (long)stopwatch.Elapsed.TotalMilliseconds;

                    allMousePoints.Add(mousePoint);

                    if (allMousePoints.Count == 1)
                    {
                        stopwatch.Start();
                    }
                }           
            }
        }
        public static Point GetMousePosition()
        {
            var point = Control.MousePosition;
            return new Point(point.X, point.Y);
        }
        public bool CanExecute()
        {
            return true;
        }
        public void ClickingButtonOnScreen()
        {
            mousePath = new MousePath();
            mousePath.allMousePoints = allMousePoints;
            if (allMousePoints[allMousePoints.Count - 1].elapsedMS > 5000)
            {
                mousePath.moveType = MovementType.RandomMovement; 
            }
            else
            {
                mousePath.moveType = MovementType.ButtonClick;
            }
            stopwatch.Restart();
           
            File.WriteAllText(filePath + "\\ " + countingFiles + ".json", JsonConvert.SerializeObject(mousePath));
            countingFiles++;
            CheckCanExecutePlayingLast();
            allMousePoints.Clear();
            Point WidthHeight = MakeButton();
            Width = (int)WidthHeight.X;
            Height = (int)WidthHeight.Y;
            Point currentPositionBtn = GetCoordinates();
            CurrentPositionX = (int)currentPositionBtn.X;
            CurrentPositionY = (int)currentPositionBtn.Y;
        }
        public Point GetCenterOfButton()
        {
            Point center = new Point((int)Width / 2, (int)Height / 2);
            Point positions = new Point((int)CurrentPositionX, (int)CurrentPositionY);
            return new Point(positions.X + center.X, positions.Y + center.Y);
            
        }
        public void PlayingLastSaved()
        {
            DirectoryInfo directoryInfo = new(folderPath + "\\" + nameOfUser);
            FileInfo lastFile = directoryInfo.GetFiles().OrderByDescending(f => f.CreationTime).FirstOrDefault();
            var t = File.ReadAllText(@"" +lastFile.FullName);
            DeserializeMousePath(t);            
        }
        public void PlayingRandomFile(string filePath)
        {
            var t = File.ReadAllText(filePath);
            Point p = GetCenterOfButton();
            /*DeserializeMousePath(t);*/
            
        }
        private void DeserializeMousePath(string mousePath)
        {
            var k = JsonConvert.DeserializeObject<MousePath>(mousePath);
            for (int i = 0; i < k.allMousePoints.Count; i++)
            {
                System.Drawing.Point p = new System.Drawing.Point((int)k.allMousePoints[i].coordinate.X, (int)k.allMousePoints[i].coordinate.Y);
                System.Drawing.Point position = p;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(position.X, position.Y);
                if (i != 0)
                {
                    Thread.Sleep((int)k.allMousePoints[i].elapsedMS - (int)k.allMousePoints[i - 1].elapsedMS);
                }
            }
        }
        public void CreateFolders()
        {
            filePath = @"C:\nenad\humanMousePath\" + nameOfUser;
            folderPath = @"C:\nenad\humanMousePath";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Directory.CreateDirectory(filePath);
            }
            else
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                else
                {
                    countingFiles = Directory.GetFiles(filePath).Length;
                    CheckCanExecutePlayingLast();
                }
            }
        }
        public void InsertingName()
        {
            if(nameOfUser.Length < 2)
            {
                System.Windows.MessageBox.Show("Enter Valid Name");
            }
            else
            {
                ShowMainUserControl = Visibility.Visible;               
                OnPropertyChanged(nameof(ShowMainUserControl));
                EnterVisibility = Visibility.Collapsed;
                CreateFolders();
                nameInserted = true;             
            }
        }
        public bool CanExecutePlayingLast()
        {
            return canExecutePlayingLast;
        }
        public void GetRandomFile()
        {
            var filePaths = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);
            Random r = new Random();
            var randomFile = filePaths[r.Next(0, filePaths.Length - 1)];
            PlayingRandomFile(randomFile);
        }
        public bool CanExecuteStartRecord()
        {
            return canExecuteStartRecord;
        }
        public bool CanExecuteStopRecord()
        {
            return canExecuteStopRecord;
        }
        public void StartRecord()
        {
            started = true;
            Point WidthHeight = MakeButton();
            Width = (int)WidthHeight.X;
            Height = (int)WidthHeight.Y;
            canExecuteStartRecord= false;
            canExecuteStopRecord = true;
        }
        public void StopRecord()
        {
            started = false;
            allMousePoints.Clear();
            canExecuteStopRecord = false;
            canExecuteStartRecord= true;
        }
        public void Testing()
        {
            MousePath mousePath = JsonConvert.DeserializeObject<MousePath>(File.ReadAllText("c:\\nenad\\humanMousePath\\ttt\\ 1.json"));
            List<MousePoint> mousePoints = new List<MousePoint>();
            mousePoints = mousePath.allMousePoints;
            List<Point> ListOfPoints = new();
            foreach (MousePoint point in mousePoints)
            {
                ListOfPoints.Add(point.coordinate);
            }
            // Define the new starting position and target center point
            Point newStartPosition = new Point(CurrentPointerX, CurrentPointerY);
            Point targetCenter = GetCenterOfButton();

            // Translate, rotate, and scale the mouse path
            List<Point> transformedPoints = TranslateRotateScalePoints(mousePoints, newStartPosition, targetCenter);
            MousePath newMousePath = new();
            newMousePath = mousePath;
            List<MousePoint> newMousePointList = new();
            
            for (int i = 0; i < newMousePath.allMousePoints.Count; i++)
            {
                MousePoint mp = new MousePoint();
                mp = newMousePath.allMousePoints[i];
                mp.elapsedMS = newMousePath.allMousePoints[i].elapsedMS;
                
                Point newPoint = new((int)transformedPoints[i].X, (int)transformedPoints[i].Y);
                mp.coordinate = newPoint;
                newMousePointList.Add(mp);
            }
            newMousePath.allMousePoints = newMousePointList;
            for (int i = 0; i < newMousePath.allMousePoints.Count; i++)
            {
                System.Drawing.Point p = new System.Drawing.Point((int)newMousePath.allMousePoints[i].coordinate.X, (int)newMousePath.allMousePoints[i].coordinate.Y);
                System.Drawing.Point position = p;
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point(position.X, position.Y);
                if (i != 0)
                {
                    Thread.Sleep((int)newMousePath.allMousePoints[i].elapsedMS - (int)newMousePath.allMousePoints[i - 1].elapsedMS);
                }
            }
            
        }
        
        private static List<Point> TranslateRotateScalePoints(List<MousePoint> mousePath, Point newStartPosition, Point targetCenter)
        {
            // Calculate the angle and distance between the old start position and the target center
            Point oldStartPosition = new Point(mousePath[0].coordinate.X, mousePath[0].coordinate.Y);
            double dx = targetCenter.X - oldStartPosition.X;
            double dy = targetCenter.Y - oldStartPosition.Y;
            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // Apply translation, rotation, and scaling to each point in the mouse path
            List<Point> transformedPoints = new List<Point>();
            foreach (MousePoint mousePoint in mousePath)
            {
                // Translate the point to the new start position
                Point translatedPoint = new Point(mousePoint.coordinate.X - oldStartPosition.X + newStartPosition.X, mousePoint.coordinate.Y - oldStartPosition.Y + newStartPosition.Y);

                // Rotate the point around the new start position
                double radians = angle * Math.PI / 180.0;
                double cos = Math.Cos(radians);
                double sin = Math.Sin(radians);
                int rotatedX = (int)Math.Round((cos * (translatedPoint.X - newStartPosition.X)) - (sin * (translatedPoint.Y - newStartPosition.Y)) + newStartPosition.X);
                int rotatedY = (int)Math.Round((sin * (translatedPoint.X - newStartPosition.X)) + (cos * (translatedPoint.Y - newStartPosition.Y)) + newStartPosition.Y);
                Point rotatedPoint = new Point(rotatedX, rotatedY);

                // Scale the point based on the distance between the old start position and the target center
                double scale = distance / Math.Sqrt(Math.Pow(targetCenter.X - mousePoint.coordinate.X, 2) + Math.Pow(targetCenter.Y - mousePoint.coordinate.Y, 2));
                int scaledX = (int)Math.Round((rotatedPoint.X - newStartPosition.X) * scale + newStartPosition.X);
                int scaledY = (int)Math.Round((rotatedPoint.Y - newStartPosition.Y) * scale + newStartPosition.Y);
                Point scaledPoint = new Point(scaledX, scaledY);       
                transformedPoints.Add(scaledPoint);
            }
            return transformedPoints;
        }

        public ICommand MouseMoving { get { return new RelayCommand(MovingMouseSimulate, CanExecute); } }
        public ICommand ClickButton { get { return new RelayCommand(ClickingButtonOnScreen, CanExecute); } }
        public ICommand PlayLastSaved { get { return new RelayCommand(PlayingLastSaved, CanExecutePlayingLast); } }
        public ICommand InsertName { get {  return new RelayCommand(InsertingName, CanExecute); } }
        public ICommand GetRandom { get { return new RelayCommand(GetRandomFile, CanExecute); } }
        public ICommand StartRecording { get { return new RelayCommand(StartRecord, CanExecuteStartRecord); } }
        public ICommand StopRecording { get { return new RelayCommand(StopRecord, CanExecuteStopRecord); } }
        public ICommand Test { get { return new RelayCommand(Testing, CanExecute); } }
    }
}
