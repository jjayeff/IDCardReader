using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
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
using Image = Google.Cloud.Vision.V1.Image;

namespace IDCardReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Face[] Face1;
        Face[] Face2;
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Config                                                          |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private readonly IFaceServiceClient _faceServiceClient = new FaceServiceClient("59cc1bff2f3348dc9c93cbbc83f1f6c9", "https://southeastasia.api.cognitive.microsoft.com/face/v1.0");
        string jsonPath = @"D:\ekkawitl\Project\idcardreader\IDCardReader\bin\Release\APIKey.json";
        string result_path = @"D:\ekkawitl\Project\idcardreader\IDCardReader\bin\Release\";
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Main                                                            |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public MainWindow()
        {
            InitializeComponent();
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Other Fucntion                                                  |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private string BrowsePhoto()
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();
            openDlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
            bool? result = openDlg.ShowDialog(this);

            if (!(bool)result)
            {
                return string.Empty;
            }

            return openDlg.FileName;
        }
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                if (strEnd == "" || !strSource.Contains(strEnd))
                    End = strSource.Length;
                else
                    End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
        private void OCRImage(string filePath)
        {
            //DetectText(filePath);
            List<string> detext_document_text = DetectDocumentText(filePath);

            // name thai
            foreach (var value in detext_document_text)
                if (getBetween(value, "ชื่อสกุล", "") != "")
                {
                    result.Text = "ชื่อและนามสกุล: " + getBetween(value, "ชื่อสกุล", "") + "\r\n";
                    break;
                }

            // name eng
            foreach (var value in detext_document_text)
            {
                if (getBetween(value, "Name", "Lastname") != "")
                {
                    result.Text += "Name: " + getBetween(value, "Name", "Lastname");
                }
                if (getBetween(value, "Lastname", "เกิด") != "")
                {
                    result.Text += " " + getBetween(value, "Lastname", "เกิด") + "\r\n";
                }
            }

            // citizen id
            foreach (var value in detext_document_text)
                if (getBetween(value, "ประจำตัวประชาชน", "") != "")
                {
                    string b = string.Empty;
                    for (int i = 0; i < detext_document_text[1].Length; i++)
                    {
                        if (Char.IsDigit(detext_document_text[1][i]))
                            b += detext_document_text[1][i];
                    }
                    if (b != "")
                    {
                        result.Text += "เลขบัตรประจำตัวประชาชน: " + b + "\r\n";
                        break;
                    }
                }

            //Date of Birth
            foreach (var value in detext_document_text)
            {
                if (getBetween(value, "เกิดวันที่", "Date") != "")
                {
                    result.Text += "เกิดวันที่: " + getBetween(value, "เกิดวันที่", "Date") + "\r\n";
                }
                if (getBetween(value, "Birth", "ศาสนา") != "")
                {
                    result.Text += "Date of Birth: " + getBetween(value, "Birth", "ศาสนา") + "\r\n";
                }
            }

            //Religion
            foreach (var value in detext_document_text)
            {
                if (getBetween(value, "ศาสนา", "ที่อยู่") != "")
                {
                    result.Text += "ศาสนา: " + getBetween(value, "ศาสนา", "ที่อยู่") + "\r\n";
                }
            }

            //Address
            foreach (var value in detext_document_text)
                if (getBetween(value, "ที่อยู่", "") != "")
                {
                    result.Text += "ที่อยู่: " + getBetween(value, "ที่อยู่", "") + "\r\n";
                    break;
                }
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Microsoft Azure Fucntion                                        |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private async Task<FaceRectangle[]> DetectTheFaceRectangles(string filePath)
        {
            try
            {
                using (var imgStream = File.OpenRead(filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imgStream);
                    var faceRectangles = faces.Select(face => face.FaceRectangle);
                    return faceRectangles.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private async Task<Face[]> DetectTheFace(string filePath)
        {
            try
            {
                using (var imgStream = File.OpenRead(filePath))
                {
                    var faces = await _faceServiceClient.DetectAsync(imgStream);
                    return faces.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private async Task<VerifyResult> VerifyFaces(Face face1, Face face2)
        {
            var result_confidence = await _faceServiceClient.VerifyAsync(face1.FaceId, face2.FaceId);
            // Verify face in the image.
            if (result_confidence == null)
            {
                faceDescriptionStatusBar.Text = "Verification result: No face detected. Please try again.";
                return null;
            }

            faceDescriptionStatusBar.Text = $"Verification result: The two faces belong to the same person. Confidence is {result_confidence.Confidence}.";
            return result_confidence;
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Google Cloud Vision Fucntion                                    |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public void DetectText(string fileName)
        {
            var credential = GoogleCredential.FromFile(jsonPath).CreateScoped(ImageAnnotatorClient.DefaultScopes);
            var channel = new Grpc.Core.Channel(ImageAnnotatorClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());
            var client = ImageAnnotatorClient.Create(channel);
            // Load the image file into memory
            var image = Image.FromFile(fileName);
            // Performs label detection on the image file
            List<string> lines = new List<string>();
            var response = client.DetectText(image);
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                {
                    lines.Add(annotation.Description);
                }

            }
            //richTextBox1.Text = lines[0];
            System.IO.File.WriteAllText(result_path + "DetectText.txt", lines[0]);
        }
        public List<string> DetectDocumentText(string fileName)
        {
            var credential = GoogleCredential.FromFile(jsonPath).CreateScoped(ImageAnnotatorClient.DefaultScopes);
            var channel = new Grpc.Core.Channel(ImageAnnotatorClient.DefaultEndpoint.ToString(), credential.ToChannelCredentials());
            var client = ImageAnnotatorClient.Create(channel);
            // Load the image file into memory
            var image = Image.FromFile(fileName);
            // Performs label detection on the image file
            List<string> lines = new List<string>();
            DetectDocumentTextBox.Text = "";
            var annotations = client.DetectDocumentText(image);
            var paragraphs = annotations.Pages
                .SelectMany(page => page.Blocks)
                .SelectMany(block => block.Paragraphs);
            foreach (var para in paragraphs)
            {
                var box = para.BoundingBox;
                Console.WriteLine($"Bounding box: {string.Join(" / ", box.Vertices.Select(v => $"({v.X}, {v.Y})"))}");
                var symbols = string.Join("", para.Words.SelectMany(w => w.Symbols).SelectMany(s => s.Text));
                Console.WriteLine($"Paragraph: {symbols}");
                lines.Add(symbols);
                DetectDocumentTextBox.Text += symbols + "\r\n";
                Console.WriteLine();
            }
            System.IO.File.WriteAllLines(result_path + "DetectDocumentText.txt", lines);
            return lines;
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | BrowseButton1 Click                                             |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private async void BrowseButton1_Click(object sender, RoutedEventArgs e)
        {
            var filePath = BrowsePhoto();
            if (filePath != string.Empty)
            {
                Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });
                Button1.IsEnabled = false;
                Button2.IsEnabled = false;
                // Draw Image
                BrowseTextBox1.Text = filePath;
                var fileUri = new Uri(filePath);
                var bitMapSource = new BitmapImage();
                bitMapSource.BeginInit();
                bitMapSource.CacheOption = BitmapCacheOption.None;
                bitMapSource.UriSource = fileUri;
                bitMapSource.EndInit();
                FaceImage1.Source = bitMapSource;

                // OCR ID Card
                OCRImage(filePath);

                // Detect the face
                FaceRectangle[] facesFound = await DetectTheFaceRectangles(filePath);
                Face1 = await DetectTheFace(filePath);
                FaceDetect1TextBox.Text = Face1.Length.ToString();
                if (Face1.Length > 0)
                {
                    //We ne drew the rectangle in each faces
                    if (facesFound.Length <= 0) return;
                    var drwVisual = new DrawingVisual();
                    var drwContext = drwVisual.RenderOpen();
                    drwContext.DrawImage(bitMapSource, new Rect(0, 0, bitMapSource.Width, bitMapSource.Height));
                    var dpi = bitMapSource.DpiX;
                    var resizeFactor = 96 / dpi;
                    foreach (var faceRect in facesFound)
                    {
                        drwContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.LimeGreen, 6),
                            new Rect(faceRect.Left * resizeFactor, faceRect.Top * resizeFactor, faceRect.Width * resizeFactor,
                                faceRect.Height * resizeFactor));
                    }
                    drwContext.Close();
                    var renderToImageCtrl = new RenderTargetBitmap((int)(bitMapSource.PixelWidth * resizeFactor),
                        (int)(bitMapSource.PixelHeight * resizeFactor), 96, 96, PixelFormats.Pbgra32);
                    renderToImageCtrl.Render(drwVisual);
                    FaceImage1.Source = renderToImageCtrl;

                    // Verify face in the image.
                    if (Face1 != null && Face2 != null && Face1.Length > 0 && Face2.Length > 0)
                        await VerifyFaces(Face1[0], Face2[0]);
                }
                else
                {
                    faceDescriptionStatusBar.Text = "Verification result: No face detected. Please try again.";
                }

                Button1.IsEnabled = true;
                Button2.IsEnabled = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = null;
                });
            }
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | BrowseButton2 Click                                             |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private async void BrowseButton2_Click(object sender, RoutedEventArgs e)
        {
            //Open File Image
            var filePath = BrowsePhoto();
            if (filePath != string.Empty)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                });
                Button1.IsEnabled = false;
                Button2.IsEnabled = false;
                // Draw Image
                BrowseTextBox2.Text = filePath;
                var fileUri = new Uri(filePath);
                var bitMapSource = new BitmapImage();
                bitMapSource.BeginInit();
                bitMapSource.CacheOption = BitmapCacheOption.None;
                bitMapSource.UriSource = fileUri;
                bitMapSource.EndInit();
                FaceImage2.Source = bitMapSource;

                // Detect the face
                FaceRectangle[] facesFound = await DetectTheFaceRectangles(filePath);
                Face2 = await DetectTheFace(filePath);
                FaceDetect2TextBox.Text = Face2.Length.ToString();
                if (Face2.Length > 0)
                {
                    //We ne drew the rectangle in each faces
                    if (facesFound.Length <= 0) return;
                    var drwVisual = new DrawingVisual();
                    var drwContext = drwVisual.RenderOpen();
                    drwContext.DrawImage(bitMapSource, new Rect(0, 0, bitMapSource.Width, bitMapSource.Height));
                    var dpi = bitMapSource.DpiX;
                    var resizeFactor = 96 / dpi;
                    foreach (var faceRect in facesFound)
                    {
                        drwContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.LimeGreen, 6),
                            new Rect(faceRect.Left * resizeFactor, faceRect.Top * resizeFactor, faceRect.Width * resizeFactor,
                                faceRect.Height * resizeFactor));
                    }
                    drwContext.Close();
                    var renderToImageCtrl = new RenderTargetBitmap((int)(bitMapSource.PixelWidth * resizeFactor),
                        (int)(bitMapSource.PixelHeight * resizeFactor), 96, 96, PixelFormats.Pbgra32);
                    renderToImageCtrl.Render(drwVisual);
                    FaceImage2.Source = renderToImageCtrl;

                    // Verify face in the image.
                    if (Face1 != null && Face2 != null && Face1.Length > 0 && Face2.Length > 0)
                        await VerifyFaces(Face1[0], Face2[0]);
                }
                else
                {
                    faceDescriptionStatusBar.Text = "Verification result: No face detected. Please try again.";
                }

                Button1.IsEnabled = true;
                Button2.IsEnabled = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mouse.OverrideCursor = null;
                });
            }
        }
    }
}
