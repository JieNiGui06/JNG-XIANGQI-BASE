using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB;
using Realms;
using JNG音乐;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using System.Net;
using System.Net.Mail;
using System.IO;

namespace 杰尼象棋
{
    /// <summary>
    /// login.xaml 的交互逻辑
    /// </summary>
    public partial class login : Window
    {
        Realm realm = Realm.GetInstance("test"); public IMongoCollection<BsonDocument> JIDs = null; public bool isconed = false;
        public IMongoDatabase Database = null;
        public BsonDocument tuser = null;
        public login()
        {
            InitializeComponent();

            
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Width = Width * (Width / g1.ActualWidth);
            Height = Height * (Height / g1.ActualHeight);
            tocon();
            if (File.Exists("login.txt"))
            {
                string myinfo = "";
                using (StreamReader reader = new StreamReader("login.txt"))
                {
                    myinfo = reader.ReadToEnd();
                }
                myinfo = myinfo.Replace("\n", "");
                string[] myinfos = myinfo.Split(new string[] { ".<split>." }, StringSplitOptions.None);
                if (myinfos.Length == 2)
                {
                    zh.Text = myinfos[0];
                    pw.Text = myinfos[1];
                    remme.IsChecked = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((!isconed && w1 == null) || !isconed)
            {
                tocon();
                return;
            }
            string id = zh.Text.Replace(" ", "");
            string password = pw.Text.Trim();
            if (id == "" || password == "")
            {
                MessageBox.Show("账号或密码错误。");
                return;
            }
            FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
            FilterDefinition<BsonDocument> filter = builderFilter.Eq("UID", id);
            List<BsonDocument> uids = JIDs.Find<BsonDocument>(filter).ToList();
            if (uids.Count == 0)
            {
                //注册新账号
                if (nick.Text.Length > 15)
                {
                    MessageBox.Show("昵称最多15个中文字符。");
                    return;
                }
                //发验证码
                
                sendid = sendid.Replace(" ", "");
                if (sendid == "")
                {
                    sendemail(id);
                    MessageBox.Show("由于您是新用户，我们已经向您发送了验证码邮件，请查收！");
                    MessageBox.Show("同时请您填写好昵称等信息。");
                    return;
                }
                
                if (sendid != yz.Text.Replace(" ",""))
                {
                    MessageBox.Show("验证码错误！");
                    return;
                }
                nick.Text = nick.Text.Replace(" ", "");
                if (nick.Text == "")
                {
                    MessageBox.Show("请填写昵称！");
                    return;
                }
                ///验证码核对后执行！！
                JIDs.InsertOne(new BsonDocument
                {
                    {"UID",id },
                    {"password",password },
                    {"nick",nick.Text },
                    {"image","" },
                    {"friends","" }
                });
            }
            else
            {
                //登入
                filter = builderFilter.And(builderFilter.Eq("UID", id), builderFilter.Eq("password", password));
                List<BsonDocument> myuid = JIDs.Find<BsonDocument>(filter).ToList();
                if (myuid.Count == 0)
                {
                    MessageBox.Show("账号或密码错误");
                    return;
                }
                tuser = myuid[0];
                MessageBox.Show("登入成功！");
                if (remme.IsChecked == true)
                {
                    using (StreamWriter writer = new StreamWriter("login.txt"))
                    {
                        writer.Write(zh.Text + ".<split>." + pw.Text);
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter("login.txt"))
                    {
                        writer.Write("");
                    }
                }
                Hide();
            }
        }
        
        public bool isclose = false;
        Window1 w1 = new Window1();
        public void tocon()
        {
            w1?.Close();
            w1 = new Window1();
            w1.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            w1.Owner = this;
            if (isclose)
                w1.WindowState = WindowState.Minimized;
            w1.Closing += (a,b) => {
                if (w1?.WindowState == WindowState.Minimized)
                    isclose = true;
                else
                    isclose = false;
                w1 = null;
            };
            w1.Show();
            Task.Run(async () =>
            {
                try
                {
                    var settings = MongoClientSettings.FromConnectionString("mongodb+srv://JNG:你的密码@jng.fy6zc.mongodb.net/?retryWrites=true&w=majority");
                    settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                    var client = new MongoClient(settings);
                    var database = client.GetDatabase("test");
                    Database = database;
                    //database.CreateCollection("WLBD",new CreateCollectionOptions);
                    JIDs = database.GetCollection<BsonDocument>("JNGUID");
                    FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                    FilterDefinition<BsonDocument> filter = builderFilter.And(builderFilter.Eq("UID", ""), builderFilter.Eq("password", ""));
                    JIDs.Find<BsonDocument>(filter).ToList();
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(async () =>
                    {
                        if (w1 != null)
                        {
                            w1.runningsign.Visibility = Visibility.Hidden;
                            if (client == null)
                            {
                                isconed = false;
                                w1.label1.Content = "连接数据库失败。";
                                w1.tipimg.Kind = PackIconKind.AlertCircle;
                            }
                            else
                            {
                                isconed = true;
                                w1.label1.Content = "连接数据库成功。";
                                w1.tipimg.Kind = PackIconKind.AlertCircleCheck;
                            }
                            w1.tipimg.Visibility = Visibility.Visible;
                            await Task.Delay(1500);
                            w1?.Close();
                        }
                    }));

                }
                catch (Exception ee)
                {
                    isconed = false;
                    w1.Title = ee.Message;
                    w1.label1.Content = "连接数据库失败。";
                    w1.tipimg.Kind = PackIconKind.AlertCircle;
                }
                await Task.Delay(1500);
                w1?.Close();
            });
        }

        string sendid = "";
        Random random = new Random();
        public bool sendemail(string mailedress)
        {
            //实例化一个发送邮件类。
            MailMessage mailMessage = new MailMessage();
            mailMessage.BodyEncoding = Encoding.GetEncoding("gb2312");
            mailMessage.HeadersEncoding = Encoding.GetEncoding("gb2312");
            //发件人邮箱地址，方法重载不同，可以根据需求自行选择。
            mailMessage.From = new MailAddress("329125460@qq.com");
            //收件人邮箱地址。
            mailMessage.To.Add(new MailAddress(mailedress));
            //抄送人邮箱地址。
            //message.CC.Add(sender);
            //邮件标题。
            mailMessage.Subject = "JNG游戏";
            //邮件内容。
            sendid = random.Next(1000, 10000).ToString();
            mailMessage.Body = "这是您的验证码：\n" + sendid;
            //是否支持内容为HTML。
            //mailMessage.IsBodyHtml = true;
            //实例化一个SmtpClient类。
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            //在这里使用的是qq邮箱，所以是smtp.qq.com，如果你使用的是126邮箱，那么就是smtp.126.com。
            //client.Host = "smtp.163.com";
            client.Host = "smtp.qq.com";
            //使用安全加密连接（是否启用SSL）
            client.EnableSsl = true;
            //设置超时时间
            //client.Timeout = 10000;
            //不和请求一块发送。
            client.UseDefaultCredentials = false;
            //验证发件人身份(发件人的邮箱，邮箱里的生成授权码);
            client.Credentials = new NetworkCredential("329125460@qq.com", "授权🐎");
            try
            {
                //发送
                client.Send(mailMessage);
                //发送成功
                return true;
            }
            catch (Exception)//发送异常
            {
                //发送失败
                //System.IO.File.WriteAllText(@"C:\test.txt", e.ToString(), Encoding.UTF8);
                return false;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (zh.Text.Replace(" ", "") != "")
            {
                sendemail(zh.Text.Replace(" ", ""));
            }
        }

        public void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (tuser != null)
            {
                Hide();
            }
            else
            {
                MessageBox.Show("登入出现问题！！");
            }
        }

        public void tmplogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("为统计人数，暂不支持。");
        }
    }
}
