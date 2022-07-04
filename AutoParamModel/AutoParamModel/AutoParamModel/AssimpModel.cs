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

namespace AutoParamModel
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
            //square();
            //door();
            house();
        }
        /// <summary>
        /// 正方形
        /// </summary>
        private void square() 
        {
            if (aiScene != null)
            {
                aiScene.Clear();
            }
            else
            {
                aiScene = new Assimp.Scene();
            }

            //构建模型 
            Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
            obj3DGroupModel.children.Add(new plugins.app.modelgenerater.CubModel());
            GroupGenerte groupGen = new GroupGenerte(obj3DGroupModel);
            groupGen.GeneratePointFace();
            this.aiScene = groupGen.tempScene;
            MessageBox.Show("参数建模成功。");
        }

        /// <summary>
        /// 门
        /// </summary>
        private void door() 
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
                //场平大小
                var width = 15;
                var length = 30;
                var height = 3;
                //围墙厚度
                var wallWidth = .5f;
                //底部坡率
                var slopeLength = .7f;
                //门宽参数
                var doorLength = 10;
                //房子参数
                var house_length = 13f;
                var house_width = 5f;
                var house_height = 4.5f;

                Obj3DGroupModel obj3DGroupModel = new Obj3DGroupModel();
                //底板
                var floor = new PrismoidModel();
                floor.upXLength = width;
                floor.upZLength = length;
                floor.height = 2f;
                floor.downXLength = width + slopeLength;
                floor.downZLength = length + slopeLength;
                floor.center = new float[] { 0, 0, 0 };
                floor.refTexturePath = "/RailWay/littlestone.png";
                obj3DGroupModel.children.Add(floor);

                //墙
                var _Frame = StandardModel.Frame(length, width, height, wallWidth, doorLength, height);
                _Frame.center = new float[] { 0, 1, 0 };
                obj3DGroupModel.children.Add(_Frame);

                //门
                var obj3DGroupModel2 = StandardModel.YardDoor(doorLength, wallWidth, height);
                obj3DGroupModel2.center = new float[] { (width - wallWidth) / 2, 1, 0 };
                obj3DGroupModel.children.Add(obj3DGroupModel2);

                //房子 
                var _house = StandardModel.House(house_length, house_width, house_height);
                _house.center = new float[] { -(width - house_width) / 2 + wallWidth + 2, 1, (length - house_length) / 2 - wallWidth - 3 };
                obj3DGroupModel.children.Add(_house);

                GroupGenerte groupGen = new GroupGenerte(obj3DGroupModel2);
                groupGen.GeneratePointFace();
                this.aiScene = groupGen.tempScene;
            }
            catch (Exception ex)
            {
            }

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

                //偏移角度
                //cub.OffsetAngle = new OffsetAngle();
                //cub.OffsetAngle.X = 113f;
                //cub.OffsetAngle.Y = 115f;
                //cub.OffsetAngle.Z = 112f;
                cub.refTexturePath = "/RailWay/littlestone.png";
                obj3DGroupModel.children.Add(cub);

                #region 左边(正方形)
                //左边(正方形)
                //var plant_width_left = 12;
                //var plant_length_left = 5;
                //var plant_height_left = 6;
                //var cub_left = new CubModel();
                //cub_left.center = new float[] { -10, 0.55f, (plant_length / 2 + plant_length / 2) / 2 };  // 前后 上下 左右
                //cub_left.width = plant_width_left;
                //cub_left.length = plant_length_left;
                //cub_left.depth = plant_height_left;
                //偏移角度
                //cub.OffsetAngle = new OffsetAngle();
                //cub.OffsetAngle.X = 113f;
                //cub.OffsetAngle.Y = 115f;
                //cub.OffsetAngle.Z = 112f;
                //cub_left.refTexturePath = "/RailWay/darkbrick.png";
                // obj3DGroupModel.children.Add(cub_left);
                #endregion

                //右边(垂直长方形)
                var plant_width_right = 10;
                var plant_length_right = 5;
                var plant_height_right = 7;
                var cub_right = new CubModel();//(plant_height_right- plant_length )/2
                cub_right.center = new float[] {0, 0.55f, -( plant_length/2+ plant_height_right/2) };  // 前后 上下 左右
                cub_right.width = plant_width_right;
                cub_right.length = plant_length_right;
                cub_right.depth = plant_height_right;
                cub_right.refTexturePath = "/RailWay/littlestone.png";
                obj3DGroupModel.children.Add(cub_right);

                //SortedSet.
                GroupGenerte groupGen = new GroupGenerte(obj3DGroupModel);
                groupGen.GeneratePointFace();
                this.aiScene = groupGen.tempScene;
        }
            catch (Exception ex)
            {

                throw ex ;
            }
        }
        /// <summary>
        /// 窗户
        /// </summary>
        private void casement() 
        {
            var casement_with = 3;
            var casement_height = 2;
            var cub_right = new CubModel();

        }
        //public static Obj3DGroupModel SortedSet1()
        //{
        //    var plant_width1 = 5;
        //    var plant_length1 = 5;
        //    var plant_height1 = 5;
        //    var cub1 = new CubModel();
        //    cub1.center = new float[] { 0, 0, 10 };  // 前后 上下 左右
        //    cub1.width = plant_width1;
        //    cub1.length = plant_length1;
        //    cub1.depth = plant_height1;
        //    //cub1.refTexturePath = "/RailWay/darkbrick.png";
        //    //obj3DGroupModel.children.Add(cub1);
        //}
    }
}
