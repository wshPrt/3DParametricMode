using Assimp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using plugins.app.ModelCommon;
using plugins.app.modelgenerater;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace model.RIM.Plant
{
    public partial class AssimpModel : Form
    {
        //当前临时场景，用于显示
        private Assimp.Scene aiScene = new Assimp.Scene();

        //显示器，用于显示模型
        open3mod.MainWindow viewWindow = new open3mod.MainWindow();
        public AssimpModel()
        {
            InitializeComponent();
            //模型显示器的初始化绑定
            viewWindow.TopLevel = false;
            viewWindow.Dock = DockStyle.Fill;
            panelViewer.Controls.Add(viewWindow);
            viewWindow.Show();

            //初始化，扫包注入基础图元
            plugins.app.modelgenerater.GeneraterConfig.Init();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (aiScene != null)
            {
                aiScene.Clear();

                MessageBox.Show("模型已清空。");
            }
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            Assimp.AssimpContext assOpretion = new Assimp.AssimpContext();
            var exportFormats = assOpretion.GetSupportedExportFormats();
            var name = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            if (!Directory.Exists(AppConfig.TempPath))
                Directory.CreateDirectory(AppConfig.TempPath);
            var filePath = AppConfig.TempPath + "\\" + name;
            bool re = false;
            var ExportFormatDescription = exportFormats.Where(x => x.FormatId.Equals("collada")).FirstOrDefault();
            if (ExportFormatDescription == null) return;
            if (aiScene.Meshes == null || aiScene.MeshCount == 0) return;
            foreach (var item in aiScene.Materials)
            {
                if (item.HasTextureDiffuse)
                {
                    TextureSlot texture = item.TextureDiffuse;
                    item.RemoveMaterialTexture(ref texture);

                    FileInfo fileInfo = new FileInfo(texture.FilePath);
                    Directory.CreateDirectory(AppConfig.TempPath + "\\" + name + "\\");
                    File.Copy(texture.FilePath, AppConfig.TempPath + "\\" + name + "\\" + fileInfo.Name + "", true);

                    Assimp.TextureSlot slot = new TextureSlot(name + "\\" + fileInfo.Name + "", TextureType.Diffuse, 0, TextureMapping.Box, 0, 0, TextureOperation.Multiply, TextureWrapMode.Wrap, TextureWrapMode.Wrap, 0);
                    item.AddMaterialTexture(ref slot);
                }
            }
            re = assOpretion.ExportFile(aiScene, filePath + "." + ExportFormatDescription.FileExtension, ExportFormatDescription.FormatId
                , PostProcessSteps.RemoveRedundantMaterials | PostProcessSteps.OptimizeMeshes | PostProcessSteps.OptimizeGraph
                );

            viewWindow.AddTab(filePath + ".dae");
        }
        private void btnGranenter_Click(object sender, EventArgs e)
        {
             house();
        }
     
        /// <summary>
        /// 房子
        /// </summary>
        private void house()
        {
            if (aiScene != null)
            {
                aiScene.Clear();
            }
            else
            {
                aiScene = new Assimp.Scene();
            }

            try
            {
                Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();

                //中间厂房大小(水平长方形)
                var plant_width = 10;
                var plant_length = 15;
                var plant_height = 6;
                var cub = new CubModel();
                cub.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
                cub.width = plant_width;
                cub.length = plant_length;
                cub.depth = plant_height;
                cub.refTexturePath = "/RailWay/skin.jpg";//#FFFAF9F5
                obj3DGroupModel.children.Add(cub);

                //右边(垂直长方形)
                var plant_width_right = 10;
                var plant_length_right = 5;
                var plant_height_right = 7;
                var cub_right = new CubModel();
                cub_right.center = new float[] { 0, 0.55f, -(plant_length / 2 + plant_height_right / 2) };  // 前后 上下 左右 //(plant_height_right- plant_length )/2
                cub_right.width = plant_width_right;
                cub_right.length = plant_length_right;
                cub_right.depth = plant_height_right;
                cub_right.refTexturePath = "/RailWay/skin.jpg";
                obj3DGroupModel.children.Add(cub_right);

                //顶层
                var flool = TopFloor();
                flool.center = new float[] { 0, plant_height / 2 + 0.1f, 0 };
                obj3DGroupModel.children.Add(flool);
                //右边顶层
                var flool_right = TopFloorRight();
                flool_right.center = new float[] { 0, plant_height_right / 2 + 0.99f , -(plant_length / 2 + plant_height_right / 2) };
                obj3DGroupModel.children.Add(flool_right);
                #region 第1行
                //第一组窗户
                var win = Windows();
                win.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) * 6};
                obj3DGroupModel.children.Add(win);
                var winOne = Windows();
                winOne.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) * 6 - 0.4f};
                obj3DGroupModel.children.Add(winOne);
                //第二组窗户
                var winTwo = Windows();
                winTwo.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) * 4 };
                obj3DGroupModel.children.Add(winTwo);
                var winThere = Windows();
                winThere.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) * 4 - 0.4f };
                obj3DGroupModel.children.Add(winThere);
                //第三组窗户
                var winFour = Windows();
                winFour.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) * 2 };
                obj3DGroupModel.children.Add(winFour);
                var winFive = Windows();
                winFive.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) * 2 - 0.4f };
                obj3DGroupModel.children.Add(winFive);
                //第四组窗户
                var winSix = Windows();
                winSix.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) - 1 };
                obj3DGroupModel.children.Add(winSix);
                var winSeven = Windows();
                winSeven.center = new float[] { plant_width / 2, (plant_height / 6) * 2, (plant_length / 12) - 1.4f };
                obj3DGroupModel.children.Add(winSeven);
                //第五组窗户
                var winEight = Windows();
                winEight.center = new float[] { plant_width / 2, (plant_height / 6) * 2, -(plant_length / 12) * 2 };
                obj3DGroupModel.children.Add(winEight);
                var winNine = Windows();
                winNine.center = new float[] { plant_width / 2, (plant_height / 6) * 2, -(plant_length / 12) * 2 - 0.4f };
                obj3DGroupModel.children.Add(winNine);
                //第六组窗户
                var win10 = Windows();
                win10.center = new float[] { plant_width / 2, (plant_height / 6) * 2, -(plant_length / 12) * 4 };
                obj3DGroupModel.children.Add(win10);
                var win11 = Windows();
                win11.center = new float[] { plant_width / 2, (plant_height / 6) * 2, -(plant_length / 12) * 4 - 0.4f };
                obj3DGroupModel.children.Add(win11);
                //第七组窗户
                var win12 = Windows();
                win12.center = new float[] { plant_width / 2, (plant_height / 6) * 2, -(plant_length / 12) * 6 };
                obj3DGroupModel.children.Add(win12);
                var win13 = Windows();
                win13.center = new float[] { plant_width / 2, (plant_height / 6) * 2, -(plant_length / 12) * 6 - 0.4f };
                obj3DGroupModel.children.Add(win13);
                //第八组窗户
                var win14 = Windows();
                win14.center = new float[] { plant_width / 2, (plant_height / 6) * 2 + 0.5f, -(plant_length / 12) * 8 - 0.5f};
                obj3DGroupModel.children.Add(win14);
                var win15 = Windows();
                win15.center = new float[] { plant_width / 2, (plant_height / 6) * 2 + 0.5f, -(plant_length / 12) * 8 - 0.9f };
                obj3DGroupModel.children.Add(win15);
                //第九组窗户
                var win16 = Windows();
                win16.center = new float[] { plant_width / 2, (plant_height / 6) * 2 + 0.5f, -(plant_length / 12) * 10 - 0.5f };
                obj3DGroupModel.children.Add(win16);
                var win17 = Windows();
                win17.center = new float[] { plant_width / 2, (plant_height / 6) * 2 + 0.5f, -(plant_length / 12) * 10 - 0.9f };
                obj3DGroupModel.children.Add(win17);
                #endregion
                #region 第2行
                //雨棚
                var awning1  = Awning1();
                awning1.center = new float[] { plant_width / 2 - 1.3f, (plant_height / 6) + 0.15f, (plant_length / 12) * 4};
                obj3DGroupModel.children.Add(awning1);
                //第一组窗户
                var win_second = Windows();
                win_second.center = new float[] { plant_width / 2, -(plant_height / 6), (plant_length / 12) * 6 };
                obj3DGroupModel.children.Add(win_second);
                var win_second1 = Windows();
                win_second1.center = new float[] { plant_width / 2, - (plant_height / 6), (plant_length / 12) * 6 - 0.4f };
                obj3DGroupModel.children.Add(win_second1);
                //第二组窗户
                var win_second2 = Windows();
                win_second2.center = new float[] { plant_width / 2, -(plant_height / 6), (plant_length / 12) * 4 };
                obj3DGroupModel.children.Add(win_second2);
                var win_second3 = Windows();
                win_second3.center = new float[] { plant_width / 2, -(plant_height / 6), (plant_length / 12) * 4 - 0.4f };
                obj3DGroupModel.children.Add(win_second3);
                //第三组窗户
                var win_second4 = Windows();
                win_second4.center = new float[] { plant_width / 2, -(plant_height / 6), (plant_length / 12) * 2 };
                obj3DGroupModel.children.Add(win_second4);
                var win_second5 = Windows();
                win_second5.center = new float[] { plant_width / 2, -(plant_height / 6), (plant_length / 12) * 2 - 0.4f };
                obj3DGroupModel.children.Add(win_second5);
                //雨棚
                var awning = Awning();
                awning.center = new float[] { plant_width / 2 - 1.3f, (plant_height / 6) + 0.15f, -(plant_length / 12) - 1.8f };
                obj3DGroupModel.children.Add(awning);
                //门
                var door = Door();
                door.center = new float[] { plant_width / 2 , -(plant_height / 6), -(plant_length / 12) - 1.1f};
                obj3DGroupModel.children.Add(door);
                var door1 = Door();
                door1.center = new float[] { plant_width / 2, -(plant_height / 6), -(plant_length / 12) + 0.23f };
                obj3DGroupModel.children.Add(door1);
                //第四组窗户
                var win_second6 = Windows();
                win_second6.center = new float[] { plant_width / 2, -(plant_height / 6), - (plant_length / 12) * 4 };
                obj3DGroupModel.children.Add(win_second6);
                var win_second7 = Windows();
                win_second7.center = new float[] { plant_width / 2, -(plant_height / 6), - (plant_length / 12) * 4 - 0.4f };
                obj3DGroupModel.children.Add(win_second7);
                //第五组窗户
                var win_second8 = Windows();
                win_second8.center = new float[] { plant_width / 2, -(plant_height / 6), -(plant_length / 12) * 6 };
                obj3DGroupModel.children.Add(win_second8);
                var win_second9 = Windows();
                win_second9.center = new float[] { plant_width / 2, -(plant_height / 6), -(plant_length / 12) * 6 - 0.4f };
                obj3DGroupModel.children.Add(win_second9);
                //门
                var door_right = Door_right();
                door_right.center = new float[] { plant_width / 2 - 0.28f, -(plant_height / 6), -(plant_length / 12) * 9 };
                obj3DGroupModel.children.Add(door_right);
                //门
                var door_right1 = Door_right();
                door_right1.center = new float[] { plant_width / 2 - 0.28f, -(plant_height / 6), -(plant_length / 12) * 11 - 0.4f };
                obj3DGroupModel.children.Add(door_right1);
                //门罢手
                //var door_handle = DoorHandle();
                //door_handle.center = new float[] { plant_width / 2 + 0.28f, -(plant_height / 6), - 1.5f };
                //obj3DGroupModel.children.Add(door_handle);
                //墙纸
                var wall_paper = WallPaper();
                wall_paper.center = new float[] { plant_length / 3 - 0.2f, -(plant_height / 6) * 2.4f, (plant_length / 6) * 2 - 0.26f };
                obj3DGroupModel.children.Add(wall_paper);
                //墙纸
                var wall_paper_right = WallPaperRight();
                wall_paper_right.center = new float[] { plant_length / 3 - 0.2f, -(plant_height / 6) * 2.4f, -(plant_length / 6) - 3.3f };
                obj3DGroupModel.children.Add(wall_paper_right);
                #endregion
                #region 左边第1行
                //第一组窗户
                var left_win = WindowsLeft();
                left_win.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 , (plant_width / 6) * 4 };
                obj3DGroupModel.children.Add(left_win);
                var left_win_ = WindowsLeft();
                left_win_.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win_.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, (plant_width / 6) * 4 - 0.4f };
                obj3DGroupModel.children.Add(left_win_);
                //第二组窗户
                var left_win1 = WindowsLeft();
                left_win1.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win1.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, (plant_width / 6) * 2};
                obj3DGroupModel.children.Add(left_win1);
                var left_win_1 = WindowsLeft();
                left_win_1.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win_1.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, (plant_width / 6) * 2 - 0.4f };
                obj3DGroupModel.children.Add(left_win_1);
                //第三组窗户
                var left_win2 = WindowsLeft();
                left_win2.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win2.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, (plant_length / 6) - 1.8f };
                obj3DGroupModel.children.Add(left_win2);
                var left_win_2 = WindowsLeft();
                left_win_2.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win_2.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, (plant_length / 6) - 2.2f };
                obj3DGroupModel.children.Add(left_win_2);
                //第四组窗户
                var left_win3 = WindowsLeft();
                left_win3.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win3.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, - (plant_length / 6) };
                obj3DGroupModel.children.Add(left_win3);
                var left_win_3 = WindowsLeft();
                left_win_3.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win_3.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, -(plant_length / 6) - 0.4f};
                obj3DGroupModel.children.Add(left_win_3);
                //第五组窗户
                var left_win4 = WindowsLeft();
                left_win4.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win4.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 , -(plant_length / 6) * 2};
                obj3DGroupModel.children.Add(left_win4);
                var left_win_4 = WindowsLeft();
                left_win_4.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_win_4.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2, -(plant_length / 6) * 2 - 0.4f};
                obj3DGroupModel.children.Add(left_win_4);
                #endregion
                #region 左边第2行
                //第一组窗户
                var left_second_win = WindowsLeft();
                left_second_win.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, (plant_width / 6) * 4 };
                obj3DGroupModel.children.Add(left_second_win);
                var left_second_win_ = WindowsLeft();
                left_second_win_.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win_.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, (plant_width / 6) * 4 - 0.4f};
                obj3DGroupModel.children.Add(left_second_win_);
                //第二组窗户
                var left_second_win1 = WindowsLeft();
                left_second_win1.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win1.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, (plant_width / 6) * 2 };
                obj3DGroupModel.children.Add(left_second_win1);
                var left_second_win_1 = WindowsLeft();
                left_second_win_1.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win_1.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, (plant_width / 6) * 2 - 0.4f};
                obj3DGroupModel.children.Add(left_second_win_1);
                //第三组窗户
                var left_second_win2 = WindowsLeft();
                left_second_win2.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win2.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, (plant_length / 6) - 1.8f };
                obj3DGroupModel.children.Add(left_second_win2);
                var left_second_win_2 = WindowsLeft();
                left_second_win_2.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win_2.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, (plant_length / 6) - 2.2f };
                obj3DGroupModel.children.Add(left_second_win_2);
                //雨棚
                var awningLeft = AwningLeft();
                awningLeft.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                awningLeft.center = new float[] { plant_length / 2 - 0.1f, (plant_height / 6) * 2 - 1.8f, (plant_length / 6) - 4.1f };
                obj3DGroupModel.children.Add(awningLeft);
                //第四组窗户
                var left_second_win3 = WindowsLeft();
                left_second_win3.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win3.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, -(plant_length / 6) };
                obj3DGroupModel.children.Add(left_second_win3);
                var left_second_win_3 = WindowsLeft();
                left_second_win_3.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win_3.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, -(plant_length / 6) - 0.4f};
                obj3DGroupModel.children.Add(left_second_win_3);
                //第五组窗户
                var left_second_win4 = WindowsLeft();
                left_second_win4.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win4.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, -(plant_length / 6) * 2 };
                obj3DGroupModel.children.Add(left_second_win4);
                var left_second_win_4 = WindowsLeft();
                left_second_win_4.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
                left_second_win_4.center = new float[] { plant_length / 2 + 0.5f, (plant_height / 6) * 2 - 2.8f, -(plant_length / 6) * 2 - 0.4f};
                obj3DGroupModel.children.Add(left_second_win_4);
                #endregion
                GroupGenerte groupGen = new GroupGenerte(obj3DGroupModel);
                groupGen.GeneratePointFace();
                this.aiScene = groupGen.tempScene;
         
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 窗户
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel Windows()
        {
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();

            var winWidth = .8f; //窗户宽
            var doorWidth = 0.05f;  //门或窗厚度

            var winWidth1 = .35f; //单个窗户宽
            var windh = 0.03f; //单个窗户厚度

            //十字架
            var _windowX = new CubModel();
            _windowX.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _windowX.width = doorWidth;
            _windowX.length = doorWidth;
            _windowX.depth = winWidth;
            _windowX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowX);

            //十字架
            var _windowY = new CubModel();
            _windowY.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _windowY.width = doorWidth;
            _windowY.length = doorWidth;
            _windowY.depth = winWidth;
            _windowY.rotate = new float[] { float.Parse((90 * Math.PI / 180).ToString()), 0, 0 };
            _windowY.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowY);

            // 上左
            var _window = SmartWindow(windh, winWidth1, -windh / 4, winWidth1 / 2 + doorWidth / 2, winWidth1 / 2 + doorWidth / 2);
            obj3DGroupModel.children.Add(_window);

            //上右
            var _window1 = SmartWindow(windh, winWidth1, -windh / 4, winWidth1 / 2 + doorWidth / 2, -(winWidth1 / 2 + doorWidth / 2));
            obj3DGroupModel.children.Add(_window1);

            //下左
            var _window2 = SmartWindow(windh, winWidth1, -windh / 4, -(winWidth1 / 2 + doorWidth / 2), winWidth1 / 2 + doorWidth / 2);
            obj3DGroupModel.children.Add(_window2);

            //下右
            var _window3 = SmartWindow(windh, winWidth1, -windh / 4, -(winWidth1 / 2 + doorWidth / 2), -(winWidth1 / 2 + doorWidth / 2));
            obj3DGroupModel.children.Add(_window3);

            //上线
            var _windowUX = new CubModel();
            _windowUX.center = new float[] { 0, winWidth1 + doorWidth, 0 };  // 前后 上下 左右
            _windowUX.width = doorWidth;
            _windowUX.length = doorWidth;
            _windowUX.depth = winWidth;
            _windowUX.rotate = new float[] { float.Parse((90 * Math.PI / 180).ToString()), 0, 0 };
            _windowUX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowUX);

            //下线
            var _windowDX = new CubModel();
            _windowDX.center = new float[] { 0, -(winWidth1 + doorWidth), 0 };  // 前后 上下 左右
            _windowDX.width = doorWidth;
            _windowDX.length = doorWidth;
            _windowDX.depth = winWidth;
            _windowDX.rotate = new float[] { float.Parse((90 * Math.PI / 180).ToString()), 0, 0 };
            _windowDX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowDX);

            //左线
            var _windowLX = new CubModel();
            _windowLX.center = new float[] { 0, 0, winWidth / 2 };  // 前后 上下 左右
            _windowLX.width = doorWidth;
            _windowLX.length = doorWidth;
            _windowLX.depth = winWidth + doorWidth;
            _windowLX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowLX);

            //右线
            var _windowRX = new CubModel();
            _windowRX.center = new float[] { 0, 0, -(winWidth / 2) };  // 前后 上下 左右
            _windowRX.width = doorWidth;
            _windowRX.length = doorWidth;
            _windowRX.depth = winWidth + doorWidth;
            _windowRX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowRX);

            return obj3DGroupModel;
        }

        /// <summary>
        /// 小玻璃窗格
        /// </summary>
        /// <param name="windh">小玻璃厚度</param>
        /// <param name="winWidth1">小玻璃宽</param>
        /// <param name="doorWidth">窗户厚度</param>
        /// <returns></returns>
        private static Obj3DGroupModel SmartWindow(float windh, float winWidth1, float x, float y, float z)
        {
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();

            var _window3 = new CubModel();
            _window3.center = new float[] { x, y, z };  // 前后 上下 左右
            _window3.width = windh;
            _window3.length = winWidth1;
            _window3.depth = winWidth1;
            _window3.refTexturePath = "/RailWay/newwindow.jpeg";
            obj3DGroupModel.children.Add(_window3);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 左边窗户
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel WindowsLeft()
        {
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();

            var winWidth = .8f; //窗户宽
            var doorWidth = 0.05f;  //门或窗厚度

            var winWidth1 = .35f; //单个窗户宽
            var windh = 0.03f; //单个窗户厚度

            //十字架
            var _windowX = new CubModel();
            _windowX.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _windowX.width = doorWidth;
            _windowX.length = doorWidth;
            _windowX.depth = winWidth;
            _windowX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowX);

            //十字架
            var _windowY = new CubModel();
            _windowY.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _windowY.width = doorWidth;
            _windowY.length = doorWidth;
            _windowY.depth = winWidth;
            _windowY.rotate = new float[] { float.Parse((90 * Math.PI / 180).ToString()), 0, 0 };
            _windowY.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowY);

            // 上左
            var _window = SmartWindow(windh, winWidth1, -windh / 4, winWidth1 / 2 + doorWidth / 2, winWidth1 / 2 + doorWidth / 2);
            _window.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
            obj3DGroupModel.children.Add(_window);

            //上右
            var _window1 = SmartWindow(windh, winWidth1, -windh / 4, winWidth1 / 2 + doorWidth / 2, -(winWidth1 / 2 + doorWidth / 2));
            _window1.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
            obj3DGroupModel.children.Add(_window1);

            //下左
            var _window2 = SmartWindow(windh, winWidth1, -windh / 4, -(winWidth1 / 2 + doorWidth / 2), winWidth1 / 2 + doorWidth / 2);
            _window2.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
            obj3DGroupModel.children.Add(_window2);

            //下右
            var _window3 = SmartWindow(windh, winWidth1, -windh / 4, -(winWidth1 / 2 + doorWidth / 2), -(winWidth1 / 2 + doorWidth / 2));
            _window3.rotate = new float[] { 0, float.Parse((90 * Math.PI / 180).ToString()), 0 };
            obj3DGroupModel.children.Add(_window3);

            //上线
            var _windowUX = new CubModel();
            _windowUX.center = new float[] { 0, winWidth1 + doorWidth, 0 };  // 前后 上下 左右
            _windowUX.width = doorWidth;
            _windowUX.length = doorWidth;
            _windowUX.depth = winWidth;
            _windowUX.rotate = new float[] { float.Parse((90 * Math.PI / 180).ToString()), 0, 0 };
            _windowUX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowUX);

            //下线
            var _windowDX = new CubModel();
            _windowDX.center = new float[] { 0, -(winWidth1 + doorWidth), 0 };  // 前后 上下 左右
            _windowDX.width = doorWidth;
            _windowDX.length = doorWidth;
            _windowDX.depth = winWidth;
            _windowDX.rotate = new float[] { float.Parse((90 * Math.PI / 180).ToString()), 0, 0 };
            _windowDX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowDX);

            //左线
            var _windowLX = new CubModel();
            _windowLX.center = new float[] { 0, 0, winWidth / 2 };  // 前后 上下 左右
            _windowLX.width = doorWidth;
            _windowLX.length = doorWidth;
            _windowLX.depth = winWidth + doorWidth;
            _windowLX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowLX);

            //右线
            var _windowRX = new CubModel();
            _windowRX.center = new float[] { 0, 0, -(winWidth / 2) };  // 前后 上下 左右
            _windowRX.width = doorWidth;
            _windowRX.length = doorWidth;
            _windowRX.depth = winWidth + doorWidth;
            _windowRX.refTexturePath = "/RailWay/winline.jpg";
            obj3DGroupModel.children.Add(_windowRX);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 小玻璃窗格
        /// </summary>
        /// <param name="windh">小玻璃厚度</param>
        /// <param name="winWidth1">小玻璃宽</param>
        /// <param name="doorWidth">窗户厚度</param>
        /// <returns></returns>
        private static Obj3DGroupModel SmartWindowLeft(float windh, float winWidth1, float x, float y, float z)
        {
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();

            var _window3 = new CubModel();
            _window3.center = new float[] { x, y, z };  // 前后 上下 左右
            _window3.width = windh;
            _window3.length = winWidth1;
            _window3.depth = winWidth1;
            _window3.refTexturePath = "/RailWay/newwindow";
            obj3DGroupModel.children.Add(_window3);

            return obj3DGroupModel;
        }
        public static Obj3DGroupModel Door() 
        {
            //门宽参数
            var doorLength = 1.3f;
            var doorDepth = 4f;
            var doorWidth = 0.18f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _door = new CubModel();
            _door.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _door.width = doorWidth;
            _door.length = doorLength;
            _door.depth = doorDepth;
            _door.refTexturePath = "/RailWay/lightBlue.jpg";
                        
            obj3DGroupModel.children.Add(_door);

            return obj3DGroupModel;
        }
        public static Obj3DGroupModel Door_right()
        {
            //门宽参数
            var doorLength = 1.8f;
            var doorDepth = 3.88f;
            var doorWidth = 0.6f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _door = new CubModel();
            _door.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _door.width = doorWidth;
            _door.length = doorLength;
            _door.depth = doorDepth;
            _door.refTexturePath = "/RailWay/lightBlue.jpg";

            obj3DGroupModel.children.Add(_door);

            return obj3DGroupModel;
        }

        /// <summary>
        /// 雨篷
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel Awning() 
        {
            var awningLength = 6f;
            var awningDepth = 0.3f;
            var awningWidth = 4f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _awning = new CubModel();
            _awning.center = new float[] { 0, 0, 0 };// 前后 上下 左右
            _awning.width = awningWidth;
            _awning.length = awningLength;
            _awning.depth = awningDepth;
            _awning.refTexturePath = "/RailWay/lightBlue.jpg";
            obj3DGroupModel.children.Add(_awning);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 雨篷1
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel Awning1()
        {
            var awningLength = 3.1f;
            var awningDepth = 0.3f;
            var awningWidth = 4f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _awning = new CubModel();
            _awning.center = new float[] { 0, 0, 0 };// 前后 上下 左右
            _awning.width = awningWidth;
            _awning.length = awningLength;
            _awning.depth = awningDepth;
            _awning.refTexturePath = "/RailWay/lightBlue.jpg";
            obj3DGroupModel.children.Add(_awning);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 雨篷2
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel AwningLeft()
        {
            var awningLength = 1.5f;
            var awningDepth = 0.3f;
            var awningWidth = 2.3f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _awning = new CubModel();
            _awning.center = new float[] { 0, 0, 0 };// 前后 上下 左右
            _awning.width = awningWidth;
            _awning.length = awningLength;
            _awning.depth = awningDepth;
            _awning.refTexturePath = "/RailWay/lightBlue.jpg";
            obj3DGroupModel.children.Add(_awning);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 中间顶层
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel TopFloor() 
        {
            var plant_width = 10.3f;
            var plant_length = 15.3f;
            var plant_height = 0.18f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _cub = new CubModel();
            _cub.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _cub.width = plant_width;
            _cub.length = plant_length;
            _cub.depth = plant_height;
            _cub.refTexturePath = "/RailWay/skin.jpg";
            obj3DGroupModel.children.Add(_cub);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 右边顶层
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel TopFloorRight()
        {
            var TopFloor_width_right = 10.5f;
            var TopFloor_length_right = 5.5f;
            var TopFloor_height_right = 0.18f; ;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _cub = new CubModel();
            _cub.center = new float[] { 0, 0, 0 };  // 前后 上下 左右
            _cub.width = TopFloor_width_right;
            _cub.length = TopFloor_length_right;
            _cub.depth = TopFloor_height_right;
            _cub.refTexturePath = "/RailWay/skin.jpg";
            obj3DGroupModel.children.Add(_cub);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 楼号
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel Round() 
        {
            var round_width = 10f;
            var round_length = 5.5f;
            var round_height = 0.23f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _round = new CubModel();
            _round.center = new float[] { 0, 0, 0 };//前后 上下 左右
            _round.width = round_width;
            _round.length = round_length;
            _round.depth = round_height;
            _round.refTexturePath = "/RailWay/RoadBed.png";
            obj3DGroupModel.children.Add(_round);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 墙纸
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel WallPaper() 
        {
            var wall_width = 0.5f;
            var wall_length = 7.5f;
            var wall_height = 1.18f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _wallPaper = new CubModel();
            _wallPaper.center = new float[] { 0, 0, 0 };//前后 上下 左右
            _wallPaper.width = wall_width;
            _wallPaper.length = wall_length;
            _wallPaper.depth = wall_height;
            _wallPaper.refTexturePath = "/RailWay/Tile.jpg";
            obj3DGroupModel.children.Add(_wallPaper);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 右边墙纸
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel WallPaperRight()
        {
            var wall_width = 0.5f;
            var wall_length = 4.8f;
            var wall_height = 1.18f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _wallPaper = new CubModel();
            _wallPaper.center = new float[] { 0, 0, 0 };//前后 上下 左右
            _wallPaper.width = wall_width;
            _wallPaper.length = wall_length;
            _wallPaper.depth = wall_height;
            _wallPaper.refTexturePath = "/RailWay/Tile.jpg";
            obj3DGroupModel.children.Add(_wallPaper);

            return obj3DGroupModel;
        }
        /// <summary>
        /// 门把手
        /// </summary>
        /// <returns></returns>
        public static Obj3DGroupModel DoorHandle() 
        {
            var handle_width = 0.1f;
            var handle_length = 0.3f;
            var handle_height = 0.1f;
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            var _handle = new CubModel();
            _handle.center = new float[] { 0, 0, 0 };//前后 上下 左右
            _handle.rotate = new float[] { 0, float.Parse((90 * Math.PI / 20).ToString()), 0 };
            _handle.width = handle_width;
            _handle.length = handle_length;
            _handle.depth = handle_height;
            _handle.refTexturePath = "/RailWay/Polished_Concrete.jpg";
            obj3DGroupModel.children.Add(_handle);

            return obj3DGroupModel;
        }
    }
}
