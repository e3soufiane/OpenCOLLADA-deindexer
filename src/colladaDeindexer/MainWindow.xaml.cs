/***********************************************
 * 
 * Author: Soufiane ERRAKI
 * Email: e3soufiane@gmail.com
 * Description: OpenCOLLADA files deindexer
 * 
 * *********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace colladaDeindexer
{
    class Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

    class Vec2
    {
        public float u;
        public float v;

        public Vec2(float _u, float _v)
        {
            u = _u;
            v = _v;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void browseButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dae"; // Default file extension

            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
                return;

            IEnumerable<float> positions;

            inputFileField.Text = dlg.FileName;

            XNamespace ns = "http://www.collada.org/2005/11/COLLADASchema";

            var doc = XDocument.Load(dlg.FileName);
            IEnumerable<XElement> triangles = doc.Descendants(ns + "triangles");
            IEnumerable<XElement> mesh = doc.Descendants(ns + "mesh");

            Vec3[] positionsDataRes;
            Vec3[] normalsDataRes;
            Vec2[] texCoordDataRes;
            Vec3[] texTangentDataRes = new Vec3[1];
            Vec3[] texBinormalDataRes = new Vec3[1];
            Vec3[] indicesDataRes;

            int elementCount = 0;

            /***********************************
                Fill Position
            */

            var inputVertex = from input in triangles.Elements(ns + "input")
                            where (string)input.Attribute("semantic") == "VERTEX"
                            select input;

            // We suppose that inputVertex and sources have one element
            var sources = inputVertex.ElementAt(0).Attributes("source");

            var verticesId = sources.ElementAt(0).Value;

            // We will use verticesId to get the position id that contains positions data
            IEnumerable<XElement> vertices = doc.Descendants(ns + "vertices");
            var inputPosition = from input in vertices.Elements(ns + "input")
                                where (string)input.Attribute("semantic") == "POSITION"
                                select input;

            var positionSourceId = inputPosition.ElementAt(0).Attribute("source");

            //positionSourceId.Value will now contain the id of positions data

            var meshSourcePosition = from meshPositions in mesh.Elements(ns + "source")
                                     where "#"+(string)meshPositions.Attribute("id") == positionSourceId.Value
                                     select meshPositions;

  
            string positionsData = meshSourcePosition.Elements(ns + "float_array").ElementAt(0).Value;
            string[] words = positionsData.Split(' ');

            positionsDataRes = new Vec3[words.Length / 3];
            for (int i = 0; i < words.Length; i+=3)
            {
                positionsDataRes[i / 3] = new Vec3(float.Parse(words[i]), float.Parse(words[i + 1]), float.Parse(words[i + 2]));
            }

            /***********************************
               Fill Normal
           */

            var inputNormal = from input in triangles.Elements(ns + "input")
                              where (string)input.Attribute("semantic") == "NORMAL"
                              select input;

            var normalSourceId = inputNormal.ElementAt(0).Attribute("source");

            var meshSourceNormal = from meshNormal in mesh.Elements(ns + "source")
                                     where "#" + (string)meshNormal.Attribute("id") == normalSourceId.Value
                                     select meshNormal;

            string normalsData = meshSourceNormal.Elements(ns + "float_array").ElementAt(0).Value;
            words = normalsData.Split(' ');

            normalsDataRes = new Vec3[words.Length / 3];
            for (int i = 0; i < words.Length; i += 3)
            {
                normalsDataRes[i / 3] = new Vec3(float.Parse(words[i]), float.Parse(words[i + 1]), float.Parse(words[i + 2]));
            }

            /***********************************
               Fill TEXCOORD
           */

            var inputTexCoord = from input in triangles.Elements(ns + "input")
                                where (string)input.Attribute("semantic") == "TEXCOORD"
                                select input;

            var texCoordSourceId = inputTexCoord.ElementAt(0).Attribute("source");

            var meshSourceTexCoord = from meshTexCoord in mesh.Elements(ns + "source")
                                     where "#" + (string)meshTexCoord.Attribute("id") == texCoordSourceId.Value
                                     select meshTexCoord;

            string texCoordData = meshSourceTexCoord.Elements(ns + "float_array").ElementAt(0).Value;
            words = texCoordData.Split(' ');

            texCoordDataRes = new Vec2[words.Length / 3];
            for (int i = 0; i < words.Length; i += 3)
            {
                texCoordDataRes[i / 3] = new Vec2(float.Parse(words[i]), float.Parse(words[i + 1]));
            }

            //modify stride value of texcoord source
            meshSourceTexCoord.Elements(ns + "technique_common").ElementAt(0).Elements(ns + "accessor").ElementAt(0).Attribute("stride").Value = (2).ToString();

            List<float> posTemp = new List<float>();
            List<float> normalTemp = new List<float>();
            List<float> texCoordTemp = new List<float>();
            List<float> texTangentTemp = new List<float>();
            List<float> texBinormalTemp = new List<float>();
            List<int> indexTemp = new List<int>();

            int gap = 3;

            if (tangentCheckbox.IsChecked == true)
            {
                gap = 5;

                /***********************************
                   Fill Tangent
               */

                var inputTexTangent = from input in triangles.Elements(ns + "input")
                                      where (string)input.Attribute("semantic") == "TEXTANGENT"
                                      select input;

                var texTangentSourceId = inputTexTangent.ElementAt(0).Attribute("source");

                var meshSourceTexTangent = from meshTexTangent in mesh.Elements(ns + "source")
                                           where "#" + (string)meshTexTangent.Attribute("id") == texTangentSourceId.Value
                                           select meshTexTangent;

                string texTangentData = meshSourceTexTangent.Elements(ns + "float_array").ElementAt(0).Value;
                words = texTangentData.Split(' ');

                texTangentDataRes = new Vec3[words.Length / 3];
                for (int i = 0; i < words.Length; i += 3)
                {
                    texTangentDataRes[i / 3] = new Vec3(float.Parse(words[i]), float.Parse(words[i + 1]), float.Parse(words[i + 2]));
                }

                /***********************************
                   Fill Binormal
               */

                var inputTexBinormal = from input in triangles.Elements(ns + "input")
                                       where (string)input.Attribute("semantic") == "TEXBINORMAL"
                                      select input;

                var texBinormalSourceId = inputTexBinormal.ElementAt(0).Attribute("source");

                var meshSourceTexBinormal = from meshTexBinormal in mesh.Elements(ns + "source")
                                            where "#" + (string)meshTexBinormal.Attribute("id") == texBinormalSourceId.Value
                                            select meshTexBinormal;

                string texBinormalData = meshSourceTexBinormal.Elements(ns + "float_array").ElementAt(0).Value;
                words = texBinormalData.Split(' ');

                texBinormalDataRes = new Vec3[words.Length / 3];
                for (int i = 0; i < words.Length; i += 3)
                {
                    texBinormalDataRes[i / 3] = new Vec3(float.Parse(words[i]), float.Parse(words[i + 1]), float.Parse(words[i + 2]));
                }

                /***********************************
                Fill Indices
                */

                string indices = triangles.Elements(ns + "p").ElementAt(0).Value;
                words = indices.Split(' ');

                indicesDataRes = new Vec3[words.Length / 3];


                var indicesDico = new Dictionary<Tuple<Tuple<Tuple<Tuple<Vec3, Vec3>, Vec2>, Vec3>, Vec3>, int>();
                for (int i = 0; i < words.Length; i += gap)
                {
                    int positionIndex = int.Parse(words[i]);
                    int normalIndex = int.Parse(words[i + 1]);
                    int texCoordIndex = int.Parse(words[i + 2]);
                    int texTangentIndex = int.Parse(words[i + 3]);
                    int texBinormalIndex = int.Parse(words[i + 4]);

                    Vec3 v1 = positionsDataRes[positionIndex];
                    Vec3 v2 = normalsDataRes[normalIndex];
                    Vec2 v3 = texCoordDataRes[texCoordIndex];
                    Vec3 v4 = texTangentDataRes[texTangentIndex];
                    Vec3 v5 = texBinormalDataRes[texBinormalIndex];

                    Tuple<Tuple<Tuple<Tuple<Vec3, Vec3>, Vec2>, Vec3>, Vec3 > tuple = Tuple.Create(Tuple.Create(Tuple.Create(Tuple.Create(v1, v2), v3),v4), v5);
                    if (indicesDico.ContainsKey(tuple))
                    {
                        indexTemp.Add(indicesDico[tuple]);
                    }
                    else
                    {
                        posTemp.Add(v1.x); posTemp.Add(v1.y); posTemp.Add(v1.z);
                        normalTemp.Add(v2.x); normalTemp.Add(v2.y); normalTemp.Add(v2.z);
                        texCoordTemp.Add(v3.u); texCoordTemp.Add(v3.v);
                        texTangentTemp.Add(v4.x); texTangentTemp.Add(v4.y); texTangentTemp.Add(v4.z);
                        texBinormalTemp.Add(v5.x); texBinormalTemp.Add(v5.y); texBinormalTemp.Add(v5.z);  
                        indexTemp.Add(elementCount);
                        indicesDico[tuple] = elementCount++;
                    }
                }

                meshSourcePosition.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", posTemp.ToArray());
                meshSourceNormal.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", normalTemp.ToArray());
                meshSourceTexCoord.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", texCoordTemp.ToArray());
                meshSourceTexTangent.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", texTangentTemp.ToArray());
                meshSourceTexBinormal.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", texBinormalTemp.ToArray());
                triangles.Elements(ns + "p").ElementAt(0).Value = string.Join(" ", indexTemp.ToArray());

                meshSourcePosition.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 3).ToString();
                meshSourceNormal.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 3).ToString();
                meshSourceTexCoord.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 2).ToString();
                meshSourceTexTangent.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 3).ToString();
                meshSourceTexBinormal.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 3).ToString();

            }
            else
            {
                /***********************************
                Fill Indices
                */

                string indices = triangles.Elements(ns + "p").ElementAt(0).Value;
                words = indices.Split(' ');

                indicesDataRes = new Vec3[words.Length / 3];

                var indicesDico = new Dictionary<Tuple<Tuple<Vec3, Vec3>, Vec2>, int>();
                for (int i = 0; i < words.Length; i += gap)
                {
                    int positionIndex = int.Parse(words[i]);
                    int normalIndex = int.Parse(words[i + 1]);
                    int texCoordIndex = int.Parse(words[i + 2]);

                    Vec3 v1 = positionsDataRes[positionIndex];
                    Vec3 v2 = normalsDataRes[normalIndex];
                    Vec2 v3 = texCoordDataRes[texCoordIndex];

                    Tuple<Tuple<Vec3, Vec3>, Vec2> tuple = Tuple.Create(Tuple.Create(v1, v2), v3);
                    if (indicesDico.ContainsKey(tuple))
                    {
 
                        indexTemp.Add(indicesDico[tuple]);
                    }
                    else
                    {

                        posTemp.Add(v1.x); posTemp.Add(v1.y); posTemp.Add(v1.z);
                        normalTemp.Add(v2.x); normalTemp.Add(v2.y); normalTemp.Add(v2.z);
                        texCoordTemp.Add(v3.u); texCoordTemp.Add(v3.v);

                        indexTemp.Add(elementCount);
                        indicesDico[tuple] = elementCount++;
                    }
                }

                meshSourcePosition.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", posTemp.ToArray());
                meshSourceNormal.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", normalTemp.ToArray());
                meshSourceTexCoord.Elements(ns + "float_array").ElementAt(0).Value = string.Join(" ", texCoordTemp.ToArray());
                triangles.Elements(ns + "p").ElementAt(0).Value = string.Join(" ", indexTemp.ToArray());

                meshSourcePosition.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 3).ToString();
                meshSourceNormal.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 3).ToString();
                meshSourceTexCoord.Elements(ns + "float_array").ElementAt(0).Attribute("count").Value = (elementCount * 2).ToString();
            }

            doc.Save(dlg.FileName + "0");

            System.Windows.MessageBox.Show("The conversion has been successful! \nOutput: \n" + dlg.FileName + "0");
        }
    }
}
