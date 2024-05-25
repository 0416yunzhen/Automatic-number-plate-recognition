using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Threading;

namespace _20231222_plateDetect
{

    public partial class Form1 : Form
    {
        //辨識車牌位置的自訂視覺服務位置及金鑰(custom-vision)(AIyunzhen-Prediction | 金鑰與端點，自訂視覺)
        const string endpoint = "https://aiyunzhen-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/c925ee2d-c771-4784-bfea-842258e21b37/detect/iterations/Iteration1/image";
        const string key = "b3d36bbeb5cf4902afa4e69541925235";
        //ocr服務的端點及金鑰(computer-vision)(AI-1-yunzhen電腦視覺)
        const string ocr_endpoint = "https://ai-1-yunzhen.cognitiveservices.azure.com/";
        const string ocr_key = "2aff8e64cf504b49b4155e7123b21c91";

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string imgPath;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                imgPath = openFileDialog1.FileName;
                pictureBox1.Image = new Bitmap(imgPath);

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Prediction-key", key);

                FileStream fileStream = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
                BinaryReader reader = new BinaryReader(fileStream);
                byte[] buffer = reader.ReadBytes((int)fileStream.Length);
                ByteArrayContent content = new ByteArrayContent(buffer);

                // 依指示將contenttype的header設為"application/octet-stream"
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // 將圖片資料透過 filestream 傳遞給 endpoint api 並等待其回傳偵測結果
                HttpResponseMessage responseMessage = await client.PostAsync(endpoint, content);

                string jsonStr = await responseMessage.Content.ReadAsStringAsync();
                richTextBox1.Text = JObject.Parse(jsonStr).ToString();

                Car_Plateinfo car_Plateinfo = JsonConvert.DeserializeObject<Car_Plateinfo>(jsonStr);
                string message = "";
                message += $"共抓到的車牌有{car_Plateinfo.predictions.Count}個\n";
                //richTextBox2.Text = $"共抓到的車牌有{car_Plateinfo.predictions.Count}個";

                Graphics g = pictureBox1.CreateGraphics();
                Pen p = new Pen(Color.Red, 1);
                int pb_width = pictureBox1.Width;
                int pb_height = pictureBox1.Height;

                int count = 0;
                double pro = 0.9;
                foreach (Prediction prediction in car_Plateinfo.predictions)
                {
                    if (prediction.probability >= pro)
                    {
                        int left = (int)(prediction.BoundingBox.left * pb_width);
                        int top = (int)(prediction.BoundingBox.top * pb_height);
                        int width = (int)(prediction.BoundingBox.width * pb_width);
                        int height = (int)(prediction.BoundingBox.height * pb_height);

                        g.DrawRectangle(p, new Rectangle(left, top, width, height));


                        count++;
                        //印出相關資訊
                        message +=
                            "Tag Name:".PadRight(15) + $"{prediction.tagname}\n" +
                            "Tag Id:".PadRight(15) + $"{prediction.tagId}\n" +
                            "Prebability:".PadRight(15) + $"{prediction.probability:p1}\n" +
                            "Left:".PadRight(15) + $"{left}\n" +
                            "Top:".PadRight(15) + $"{top}\n" +
                            "width:".PadRight(15) + $"{width}\n" +
                            "Height:".PadRight(15) + $"{height}\n\n";

                    }
                }
                message += $"共有{count}個車牌的信心度大於{pro:p}";
                fileStream.Close();

                //ocr
                fileStream = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
                ComputerVisionClient visionClient = new ComputerVisionClient(
                    new ApiKeyServiceClientCredentials(ocr_key),
                    new System.Net.Http.DelegatingHandler[] {}
                    );
                visionClient.Endpoint = ocr_endpoint;

                ReadInStreamHeaders textHeaders = await visionClient.ReadInStreamAsync(fileStream);
                string operationLocation = textHeaders.OperationLocation;
                Thread.Sleep(2000);
                string operationId = operationLocation.Substring(operationLocation.Length - 36);//取substring並扣掉後面36碼不是operation部分

                ReadOperationResult result = await visionClient.GetReadResultAsync(Guid.Parse(operationId));
                IList<ReadResult> textUrlFileResults = result.AnalyzeResult.ReadResults;
                string str = "";
                foreach(ReadResult textResult in textUrlFileResults)
                {
                    foreach(Line line in textResult.Lines)
                    {
                        str += line.Text + "\n";
                    }
                }

                message += "\n\n車牌號碼:\t" + str + "\n";


                richTextBox2.Text = message;


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
