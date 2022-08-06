using JNG音乐;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Driver;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace 杰尼象棋
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public DispatcherTimer wtftimer = new DispatcherTimer();
        public DispatcherTimer msgtimer = new DispatcherTimer();
        public DispatcherTimer updatetimer = new DispatcherTimer();
        public DispatcherTimer fisonlinetimer = new DispatcherTimer();//60秒一次检查同时更新好友列表
        IMongoCollection<BsonDocument> JIDs; public bool isconed = true;
        IMongoDatabase database; IMongoCollection<BsonDocument> rooms; IMongoCollection<BsonDocument> fs; IMongoCollection<BsonDocument> msg;
        IMongoCollection<BsonDocument> jngonline;
        public BsonDocument tuser = null;

        int o_g = 0;
        public MainWindow()
        {
            InitializeComponent();

            wtftimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            wtftimer.Tick += Timer1_Tick;
            wtftimer.IsEnabled = true;

            msgtimer.Interval = new TimeSpan(0, 0, 0, 1);
            msgtimer.Tick += Msgtimer_Tick; ;
            msgtimer.IsEnabled = true;

            updatetimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            //updatetimer.Tick += Timer1_Tick;
            updatetimer.IsEnabled = true;

            fisonlinetimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            //fisonlinetimer.Tick += Msgtimer_Tick; ;
            fisonlinetimer.IsEnabled = true;
        }

        int ashoststate = 0;//0:yes 1:no
        bool isover = true;
        private async void Msgtimer_Tick(object sender, EventArgs e)
        {
            if (tuser == null || !isover)
                return;
            
            try
            {
                FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                FilterDefinition<BsonDocument> filter = builderFilter.Eq("UID2", tuser.GetElement("UID").Value.ToString());
                List<BsonDocument> msgs = (await msg.FindAsync(filter)).ToList();
                foreach (var ms in msgs)
                {
                    string cback = ms.GetElement("cback").Value.ToString();

                    if (cback == "-1")
                    {
                        isover = false;
                        login lo = new login();
                        setmsgwindow(lo);
                        lo.Title = ms.GetElement("title").Value.ToString();
                        lo.ltip.Content = new TextBlock() { Text = ms.GetElement("body").Value.ToString() };
                        lo.tmplogin.Content = "取消";
                        lo.login1.Content = "接受";
                        lo.tmplogin.Click += (a, b) =>
                        {
                            if (ms.GetElement("type").Value.ToString() == "0")
                                msg.DeleteOne(ms);
                            else
                            {
                                var up = Builders<BsonDocument>.Update.Set("cback", "1");
                                msg.UpdateOne(ms, up);
                            }
                            lo.Close();
                        };
                        lo.login1.Click += (a, b) =>
                        {
                            if (ms.GetElement("type").Value.ToString() == "0")
                                msg.DeleteOne(ms);
                            else
                            {
                            //返回0，答应并搜索加入房间
                                var up = Builders<BsonDocument>.Update.Set("cback", "0");
                                msg.UpdateOne(ms, up);
                                Button_Click_1(null, null);
                                foreach (ListBoxItem lsti in room_f.Items)
                                {
                                    var element = lsti.Tag as BsonDocument;
                                    var update = Builders<BsonDocument>.Update.Set("UID2", tuser.GetElement("UID").Value.ToString());
                                    update = update.Set("isstart", "1");
                                    rooms.UpdateOne(element, update);
                                    fs.InsertOneAsync(new BsonDocument
                                    {
                                {"UID1" ,element.GetElement("UID1").Value.ToString()},
                                {"UID2",tuser.GetElement("UID").Value.ToString() },
                                {"fstr","" },
                                {"turnto","0" },
                                {"isstart","1"}
                                    });

                                //渲染+开始
                                    isowner = false;
                                    textwheretofight.Text = "您和" + element.GetElement("Unick").Value.ToString() + "(" + element.GetElement("UID1").Value.ToString() + ")的战斗。";
                                    setowneringtmage();
                                    setguestingtmage(JIDs.Find(new BsonDocument { { "UID", element.GetElement("UID1").Value.ToString() } }).ToList()[0]);
                                    
                                //隐藏菜单
                                    menu_.Visibility = Visibility.Hidden;
                                    break;
                                }
                            }
                            lo.Close();
                        };
                        lo.Closed += (a, b) =>
                        {
                            isover = true;
                        };
                        lo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        lo.Owner = this;
                        lo.ShowDialog();
                    }
                }

                //host部分
                filter = builderFilter.Eq("UID1", tuser.GetElement("UID").Value.ToString());
                msgs = (await msg.FindAsync(filter)).ToList();
                foreach (var ms in msgs)
                {
                    string cback = ms.GetElement("cback").Value.ToString();
                    if (cback == "0")//作为host，guest同意了
                    {
                        await msg.DeleteOneAsync(ms);
                    }
                    if (cback == "1")//refuse
                    {//退出等待
                        ashoststate = 1;
                        await msg.DeleteOneAsync(ms);
                    }
                }
            }
            catch (Exception ee)
            { }
        }

        string lastsetstring = "";
        bool tfirst = true;
        private async void Timer1_Tick(object sender, EventArgs e)
        {
            if (tuser == null)
                return;
            try
            {
                if (tfirst)
                {
                    tfirst = false;
                    fs.DeleteMany(new BsonDocument { { "UID1", tuser.GetElement("UID").Value.ToString() } });
                    fs.DeleteMany(new BsonDocument { { "UID2", tuser.GetElement("UID").Value.ToString() } });
                }
                if (isowner)
                {
                    FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                    FilterDefinition<BsonDocument> filter = builderFilter.And(builderFilter.Eq("UID1", tuser.GetElement("UID").Value.ToString()), builderFilter.Eq("isstart", "1"));
                    List<BsonDocument> fstrs = (await fs.FindAsync(filter)).ToList();
                    if (fstrs.Count != 0 && fstrs[0].GetElement("turnto").Value.ToString() == "0")
                    {
                        string thisfstr = fstrs[0].GetElement("fstr").Value.ToString();
                        if (lastsetstring != thisfstr)
                        {
                            lastsetstring = thisfstr;
                            setbtnbystring(thisfstr);
                            canownergo = true;
                            o_g = int.Parse(fstrs[0].GetElement("turnto").Value.ToString());
                        }
                    }
                }
                else
                {
                    FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                    FilterDefinition<BsonDocument> filter = builderFilter.And(builderFilter.Eq("UID2", tuser.GetElement("UID").Value.ToString()), builderFilter.Eq("isstart", "1"));
                    List<BsonDocument> fstrs = (await fs.FindAsync(filter)).ToList();
                    if (fstrs.Count != 0 && fstrs[0].GetElement("turnto").Value.ToString() == "1")
                    {
                        //MessageBox.Show("");
                        string thisfstr = fstrs[0].GetElement("fstr").Value.ToString();
                        if (lastsetstring != thisfstr)
                        {
                            lastsetstring = thisfstr;
                            setbtnbystring(thisfstr);
                            canownergo = false;
                            o_g = int.Parse(fstrs[0].GetElement("turnto").Value.ToString());
                        }
                    }
                }
            }
            catch { }
        }

        Button selbtn = null;//将移动的棋子
        List<Button> qis = new List<Button>();//此实例将操作的所有棋子
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Width = Width * (Width / g1.ActualWidth);
            //Height = Height * (Height / g1.ActualHeight);
            int index = 0;
            foreach (UIElement uI in g1.Children)
            {
                if (uI is Button)
                {
                    Button button = (Button)uI;
                    qis.Add(button);
                    button.Name = button.Tag.ToString() + index.ToString();
                    button.Tag = index;

                    button.BorderThickness = new Thickness(0);
                    button.Click += (a, b) =>
                    {
                        int myindex = index;
                        foreach (Button nb in qis)
                        {
                            if (button.BorderThickness == new Thickness(1))
                                nb.BorderThickness = new Thickness(0);
                        }
                        button.BorderThickness = new Thickness(1);
                        if ((int)(a as Button).Tag <= 15)
                        {//红方棋
                            if (isowner && tmpls[0] == null)
                            {
                                selbtn = button;
                                Label l = new Label();
                                l.HorizontalAlignment = HorizontalAlignment.Left;
                                l.VerticalAlignment = VerticalAlignment.Top;
                                l.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                                l.Margin = new Thickness(0, button.Margin.Top, 0, 0);
                                l.Height = button.Height;l.Width = Width;

                                Label l2 = new Label();
                                l2.HorizontalAlignment = HorizontalAlignment.Left;
                                l2.VerticalAlignment = VerticalAlignment.Top;
                                l2.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                                l2.Margin = new Thickness(button.Margin.Left, 0, 0, 0);
                                l2.Height = Height;l2.Width = button.Width;

                                tmpls = new Label[] { l, l2 };
                                g1.Children.Add(l);g1.Children.Add(l2);
                            }
                        }
                        else
                        {//黑方棋
                            if (!isowner && tmpls[0] == null)
                            {
                                selbtn = button;

                                Label l = new Label();
                                l.HorizontalAlignment = HorizontalAlignment.Left;
                                l.VerticalAlignment = VerticalAlignment.Top;
                                l.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                                l.Margin = new Thickness(0, button.Margin.Top, 0, 0);
                                l.Height = button.Height; l.Width = Width;

                                Label l2 = new Label();
                                l2.HorizontalAlignment = HorizontalAlignment.Left;
                                l2.VerticalAlignment = VerticalAlignment.Top;
                                l2.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                                l2.Margin = new Thickness(button.Margin.Left, 0, 0, 0);
                                l2.Height = Height; l2.Width = button.Width;

                                tmpls = new Label[] { l, l2 };
                                g1.Children.Add(l); g1.Children.Add(l2);
                            }
                        }
                    };
                    index++;
                }
            }
            //setbtnbystring("b9:308,371@");
            login loginwindow = new login();
            loginwindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            loginwindow.Owner = this;
            loginwindow.ShowDialog();
            if (loginwindow.JIDs == null || loginwindow.Database == null)
            {
                MessageBox.Show("登入出问题！！");
                Close();
            }
            else
            {
                try
                {
                    database = loginwindow.Database;
                    rooms = database.GetCollection<BsonDocument>("JNGXQrooms");
                    fs = database.GetCollection<BsonDocument>("JNGXQfs");
                    msg = database.GetCollection<BsonDocument>("JNGXQmsg");
                    jngonline = database.GetCollection<BsonDocument>("JNGonline");
                    JIDs = loginwindow.JIDs;
                    tuser = loginwindow.tuser;
                }
                catch { Close(); }
            }
            //刷新
            Button_Click_1(null, null);
        }

        public void setsingletextwindow(login lo)
        {
            lo.tmplogin.Visibility = Visibility.Hidden;
            lo.tpagain.Visibility = Visibility.Hidden;
            lo.remme.Visibility = Visibility.Hidden;
            lo.lrme.Visibility = Visibility.Hidden;
            lo.lyz.Visibility = Visibility.Hidden;
            lo.yz.Visibility = Visibility.Hidden;
            lo.pw.Visibility = Visibility.Hidden;
            lo.lpas.Visibility = Visibility.Hidden;
            lo.nick.Visibility = Visibility.Hidden;
            lo.lnick.Visibility = Visibility.Hidden;
            lo.Loaded -= lo.Window_Loaded;
            lo.Closing -= lo.Window_Closing;
            lo.login1.Click -= lo.Button_Click;
        }

        /// <summary>
        /// <paramref name="lo"/>
        /// </summary>
        /// <param name="lo"></param>
        /// <param name="type">0=>普通信息 1=>邀请信息</param>
        public void setmsgwindow(login lo, int type = 0)
        {
            //lo.tmplogin.Visibility = Visibility.Hidden;
            lo.tpagain.Visibility = Visibility.Hidden;
            lo.remme.Visibility = Visibility.Hidden;
            lo.lrme.Visibility = Visibility.Hidden;
            lo.lyz.Visibility = Visibility.Hidden;
            lo.yz.Visibility = Visibility.Hidden;
            lo.pw.Visibility = Visibility.Hidden;
            lo.lpas.Visibility = Visibility.Hidden;
            lo.nick.Visibility = Visibility.Hidden;
            lo.lnick.Visibility = Visibility.Hidden;
            lo.zh.Visibility = Visibility.Hidden;
            lo.lyx.Visibility = Visibility.Hidden;
            lo.ltip.Height = lo.Height - 70;
            lo.Loaded -= lo.Window_Loaded;
            lo.Closing -= lo.Window_Closing;
            lo.login1.Click -= lo.Button_Click;
            lo.tmplogin.Click -= lo.tmplogin_Click;
        }


        bool thisstart = false;bool isashostgetother = false;string ashostgetUID2 = "", ashosttitle = "", ashostbody = "", ashosttype = "";
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                login lo = new login();
                setsingletextwindow(lo);
                lo.Title = "创建房间";
                lo.ltip.Content = "创建一个房间。";
                lo.login1.Content = "创建";
                lo.lyx.Content = "名称";
                BsonDocument bsonElements = new BsonDocument
                {
                    {"Rnick",lo.zh.Text },
                    {"Unick",tuser.GetElement("nick").Value.ToString() },
                    {"isstart","0" },
                    {"UID1",tuser.GetElement("UID").Value.ToString() },
                    {"UID2","" }
                };
                bool isclose = true;
                lo.login1.Click += (a, b) =>
                {
                    if (lo.zh.Text.Replace(" ", "") == "")
                    {
                        MessageBox.Show("不能为空");
                        return;
                    }
                    fs.DeleteManyAsync(new BsonDocument
                    {
                    {"UID1",tuser.GetElement("UID").Value.ToString() }
                    });
                    rooms.DeleteManyAsync(new BsonDocument
                    {
                    {"UID1",tuser.GetElement("UID").Value.ToString() }
                    });
                    rooms.InsertOneAsync(new BsonDocument
                    {
                    {"Rnick",lo.zh.Text },
                    {"Unick",tuser.GetElement("nick").Value.ToString() },
                    {"isstart","0" },
                    {"UID1",tuser.GetElement("UID").Value.ToString() },
                    {"UID2","" }
                    });
                    isclose = false;
                    lo.Close();
                };

                lo.Closing += (a, b) =>
                {
                //isclose = true;
                };

                lo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                lo.Owner = this;
                lo.ShowDialog();
                if (isclose)
                {
                    msg.DeleteManyAsync(new BsonDocument { { "UID1", tuser.GetElement("UID").Value.ToString() } });
                    return;
                }
                //渲染等待界面
                Window1 w1 = new Window1();
                w1.label1.Content = "正在等待加入。。。";
                //判断作为host有没有邀请别人
                if (isashostgetother)
                {
                    isashostgetother = false;
                    //那么发邀请消息
                    msg.InsertOneAsync(new BsonDocument
                {
                    {"UID1",tuser.GetElement("UID").Value.ToString() },
                    {"UID2",ashostgetUID2 },
                    {"title",ashosttitle },
                    {"body",ashostbody },
                    {"type",ashosttype },
                    {"cback","-1" }
                });
                }
                else
                {
                    //msg.DeleteManyAsync(new BsonDocument { { "UID1", tuser.GetElement("UID").Value.ToString() } });
                }
                w1.Closing += (a, b) =>
                {
                    if (!thisstart)
                    {
                    //删除房间
                    //显示此窗口
                        w1.timer1.IsEnabled = false;
                        rooms.DeleteMany(new BsonDocument{
                        { "Unick",tuser.GetElement("nick").Value.ToString() },
                        { "UID1",tuser.GetElement("UID").Value.ToString() }
                            });

                        thisstart = false;
                        menu_.Visibility = Visibility.Visible;
                        Show();
                    }
                };
                //tick委托
                w1.timer1.Interval = new TimeSpan(0, 0, 0, 0, 1000);
                w1.timer1.Tick += async (a, b) =>
                {
                //邀请被拒绝
                    if (ashoststate == 1)
                    {
                        ashoststate = 0;

                        menu_.Visibility = Visibility.Hidden;
                        w1.timer1.IsEnabled = false;
                        w1?.Close();
                        Show();
                        MessageBox.Show("对方拒绝了。");
                        await msg.DeleteManyAsync(new BsonDocument { { "UID1", tuser.GetElement("UID").Value.ToString() } });
                        return;
                    }
                //完成后关闭并显示此窗口
                    BsonDocument newbson = (await rooms.FindAsync(new BsonDocument
                    {
                    {"Unick",tuser.GetElement("nick").Value.ToString() },
                    {"UID1",tuser.GetElement("UID").Value.ToString() }
                    })).ToList()[0];
                    
                    if (newbson.GetElement("isstart").Value.ToString() == "1")
                    {
                    //开始了
                        var that = (await JIDs.FindAsync(new BsonDocument { { "UID", newbson.GetElement("UID2").Value.ToString() } })).ToList()[0];
                        isowner = true;
                        textwheretofight.Text = "您和" + that.GetElement("nick").Value.ToString() + "(" + newbson.GetElement("UID2").Value.ToString() + ")的战斗。";
                        setowneringtmage();
                        setguestingtmage((await JIDs.FindAsync(new BsonDocument { { "UID", newbson.GetElement("UID2").Value.ToString() } })).ToList()[0]);
                        menu_.Visibility = Visibility.Hidden;
                        thisstart = true;
                        w1.timer1.IsEnabled = false;
                        w1?.Close();
                        Show();
                        await msg.DeleteManyAsync(new BsonDocument { { "UID1", tuser.GetElement("UID").Value.ToString() } });
                    }
                };
                w1.timer1.IsEnabled = true;
                Hide();
                w1.Show();
            }
            catch { }
        }

        //@button.tag:left,top@
        //button.name = type of qi
        //name=> j：将军s：士x：相m：🐎c：鸡/车p：炮b：兵
        //turn 0:owner,1:guest
        //区分敌我：[0,15](15,31]
        public bool isowner = true;
        public async void ownerfight(string f)
        {
            if (f.Replace(" ", "") == "")
                return;
            try
            {
                canownergo = false;
                FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                FilterDefinition<BsonDocument> filter = builderFilter.Eq("UID1", tuser.GetElement("UID").Value.ToString());
                var tmpmyfstr = await fs.FindAsync(filter);
                List<BsonDocument> myfstr = tmpmyfstr.ToList();
                if (myfstr.Count == 0)
                {
                    await fs.InsertOneAsync(new BsonDocument
                {
                    {"UID1" ,tuser.GetElement("UID").Value.ToString()},
                    {"UID2","" },
                    {"fstr",f },
                    {"turnto","0" },
                    {"isstart","1" }
                });
                }
                else
                {
                    var update = Builders<BsonDocument>.Update.Set("fstr", f);
                    update = update.Set("turnto", "1");
                    await fs.UpdateOneAsync(filter, update);
                }
            }
            catch { }
        }

        public async void guestfight(string f)
        {
            if (f.Replace(" ", "") == "")
                return;
            try
            {
                canownergo = true;
                FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                FilterDefinition<BsonDocument> filter = builderFilter.Eq("UID2", tuser.GetElement("UID").Value.ToString());
                var update = Builders<BsonDocument>.Update.Set("fstr", f);
                update = update.Set("turnto", "0");
                update = update.Set("UID2", tuser.GetElement("UID").Value.ToString());
                await fs.UpdateOneAsync(filter, update);
            }
            catch { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //刷新
                FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                FilterDefinition<BsonDocument> filter = builderFilter.Eq("UID2", "");
                List<BsonDocument> bsons = rooms.Find<BsonDocument>(filter).ToList();
                room_f.Items.Clear();
                foreach (BsonDocument element in bsons)
                {
                    ListBoxItem item = new ListBoxItem();
                    //Grid grid = new Grid();
                    item.Content = element.GetElement("Unick").Value.ToString() + "的 " + element.GetElement("Rnick").Value.ToString();
                    item.Tag = element;
                    //item.Foreground = Brushes.White;
                    item.MouseDoubleClick += (a, b) =>
                    {
                    //MessageBox.Show(tuser.GetElement("UID").ToString());
                        var update = Builders<BsonDocument>.Update.Set("UID2", tuser.GetElement("UID").Value.ToString());
                        update = update.Set("isstart", "1");
                        rooms.UpdateOneAsync(element, update);
                        fs.InsertOneAsync(new BsonDocument
                        {
                        {"UID1" ,element.GetElement("UID1").Value.ToString()},
                        {"UID2",tuser.GetElement("UID").Value.ToString() },
                        {"fstr","" },
                        {"turnto","0" },
                        {"isstart","1"}
                        });
                    //渲染+开始
                        isowner = false;
                        textwheretofight.Text = "您和" + element.GetElement("Unick") + "(" + element.GetElement("UID1").Value.ToString() + ")的战斗。";
                        setowneringtmage();
                        Task.Run(() => setguestingtmage(JIDs.FindAsync(new BsonDocument { { "UID", element.GetElement("UID1").Value.ToString() } }).Result.ToList()[0]));
                        

                    //隐藏菜单
                        menu_.Visibility = Visibility.Hidden;
                    };
                    room_f.Items.Add(item);
                }
            }
            catch { }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {//跟随移动
            if (selbtn != null && tmpls[0] != null && tmpls[1] != null)
            {
                Point mympoint = e.GetPosition(g1);
                double dif = (mympoint.X - 51.5f) % 72;
                double myx = mympoint.X - dif;

                double dify = (mympoint.Y - 53.5f) % 72;
                double myy = mympoint.Y - dify;
                tmpls[0].Margin = new Thickness(0, (int)((mympoint.Y - 23) / 70) * 70 + 23, 0, 0);
                tmpls[1].Margin = new Thickness((int)((mympoint.X - 21)/72) * 72 + 21, 0, 0, 0);
            }
        }

        Label[] tmpls = new Label[2];
        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {//del
            deltmpls();
        }
        public void deltmpls()
        {
            for (int i = tmpls.Length - 1; i > -1; i--)
            {
                if (tmpls[i] != null)
                {
                    g1.Children.Remove(tmpls[i]);
                    tmpls[i] = null;
                }
            }
            selbtn = null;
        }

        public Rect getrect(Label l)
        {
            return new Rect(l.Margin.Left, l.Margin.Top, l.Width, l.Height);
        }
        public Rect getrect(Button l)
        {
            return new Rect(l.Margin.Left, l.Margin.Top, l.Width, l.Height);
        }

        public bool istouchwithbtns(Rect r)
        {
            foreach (Button b in qis)
            {
                if (b.Visibility == Visibility.Visible && getrect(b).IntersectsWith(r))
                {
                    return true;
                }
            }
            return false;
        }

        public int itoeachfrompoint(Rect r)
        {
            int i = 0;
            foreach (Button b in qis)
            {
                if (b.Visibility == Visibility.Visible && getrect(b).IntersectsWith(r))
                {
                    i++;
                }
            }
            return i;
        }

        bool canownergo = true;
        private async void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {//处理位置+del
            //name=> j：将军s：士x：相m：🐎c：鸡/车p：炮b：兵
            //判断轮到谁，并设置 turnto
            ERROR1:
            if (selbtn != null && tmpls[0] != null && tmpls[1] != null)
            {
                try
                {
                    FilterDefinitionBuilder<BsonDocument> builderFilter = Builders<BsonDocument>.Filter;
                    FilterDefinition<BsonDocument> filter = builderFilter.And(builderFilter.Eq("UID1", tuser.GetElement("UID").Value.ToString()), builderFilter.Eq("isstart", "1"));
                    var fstr_ = await fs.FindAsync<BsonDocument>(filter);
                    List<BsonDocument> fstrs = fstr_.ToList();
                    filter = builderFilter.And(builderFilter.Eq("UID2", tuser.GetElement("UID").Value.ToString()), builderFilter.Eq("isstart", "1"));
                    fstr_ = await fs.FindAsync<BsonDocument>(filter);
                    var fstrs1 = fstr_.ToList();
                    if ((isowner && fstrs[0].GetElement("turnto").Value.ToString() == "0") || (!isowner && fstrs1[0].GetElement("turnto").Value.ToString() == "1"))
                    {
                        o_g = -1;

                        //if (selbtn.Name.Contains("j"))
                        //{
                        Thickness lastt = selbtn.Margin;
                        Point mympoint = e.GetPosition(g1);
                        Rect r1 = getrect(tmpls[0]);
                        Rect r2 = getrect(tmpls[1]);
                        r1.Intersect(r2);
                        bool ishitmyf = false;
                        //吃///先判断敌我
                        for (int i = qis.Count - 1; i > -1; i--)
                        {
                            Rect br1 = getrect(qis[i]);
                            Rect br2 = r1;
                            if (br1.IntersectsWith(br2) && qis[i].Visibility == Visibility.Visible && qis[i] != selbtn)
                            {//吃（隐藏）
                             //判断有没有踩到队友
                                if (((int)qis[i].Tag) < 16 && ((int)selbtn.Tag) > 15 || ((int)qis[i].Tag) > 15 && ((int)selbtn.Tag) < 16)
                                {
                                    //qis[i].Visibility = Visibility.Hidden;
                                }
                                else
                                {//踩到队友
                                    ishitmyf = true;
                                }
                                //判断是不是帅，是否输嘞！

                            }
                        }
                        //判断有没有踩到队友
                        //name=> j：将军s：士x：相m：🐎c：鸡/车p：炮b：兵
                        if (!ishitmyf)
                        {//向右
                            double off = (r1.Left - selbtn.Margin.Left) / 72;
                            int offest = (int)Math.Round(off, 0);
                            int len = (int)Math.Sqrt(Math.Pow(r1.Left - selbtn.Margin.Left, 2) + Math.Pow(r1.Top - selbtn.Margin.Top, 2));
                            if (((selbtn.Name.Contains("j")) && (len > 80))   //如果这样，不走并发出警报
                                || ((selbtn.Name.Contains("j")) && (isowner) && (r1.Top < 480 || r1.Left < 206 || r1.Left > 410))
                                || ((selbtn.Name.Contains("j")) && (!isowner) && (r1.Top > 190 || r1.Left < 206 || r1.Left > 410))
                                || ((selbtn.Name.Contains("s")) && ((len > 120) || (len < 80)))
                                || ((selbtn.Name.Contains("s")) && ((r1.Left < 200) || (r1.Left > 410)))
                                || ((selbtn.Name.Contains("s")) && (isowner) && (r1.Top < 480))
                                || ((selbtn.Name.Contains("s")) && (!isowner) && (r1.Top > 200))
                                || ((selbtn.Name.Contains("x")) && (Math.Abs(offest) != 2))
                                || ((selbtn.Name.Contains("x")) && (istouchwithbtns(new Rect((r1.Left + selbtn.Margin.Left) / 2, (r1.Top + selbtn.Margin.Top) / 2, 1, 1))))
                                || ((selbtn.Name.Contains("x")) && (r1.Top < 330) && (isowner))
                                || ((selbtn.Name.Contains("x")) && (r1.Top > 330) && (!isowner))
                                || ((selbtn.Name.Contains("x")) && (len > 220))
                                || ((selbtn.Name.Contains("m")) && ((len < 150) || (len > 180)))
                                || (selbtn.Name.Contains("m") && (Math.Abs(offest) == 1) && (r1.Top < selbtn.Margin.Top) && (istouchwithbtns(new Rect(selbtn.Margin.Left, selbtn.Margin.Top - 70, 61, 61))))
                                || ((selbtn.Name.Contains("m")) && (Math.Abs(offest) == 1) && (r1.Top > selbtn.Margin.Top) && (istouchwithbtns(new Rect(selbtn.Margin.Left, selbtn.Margin.Top + 70, 61, 61))))
                                || ((selbtn.Name.Contains("m")) && (Math.Abs(offest) == 2) && (r1.Left < selbtn.Margin.Left) && (istouchwithbtns(new Rect(selbtn.Margin.Left - 72, selbtn.Margin.Top, 61, 61))))
                                || ((selbtn.Name.Contains("m")) && (Math.Abs(offest) == 2) && (r1.Left > selbtn.Margin.Left) && (istouchwithbtns(new Rect(selbtn.Margin.Left + 72, selbtn.Margin.Top, 61, 61))))
                                || ((selbtn.Name.Contains("c")) && (istouchwithbtns(r1)) && (offest == 0) && (itoeachfrompoint(new Rect(r1.Left, r1.Top < selbtn.Margin.Top ? r1.Top : selbtn.Margin.Top + 30, 61, Math.Abs(selbtn.Margin.Top - r1.Top))) != 2))
                                || ((selbtn.Name.Contains("c")) && (istouchwithbtns(r1)) && (offest != 0) && (itoeachfrompoint(new Rect(r1.Left < selbtn.Margin.Left ? r1.Left : selbtn.Margin.Left + 30, r1.Top, Math.Abs(r1.Left - selbtn.Margin.Left), 61)) != 2))
                                || ((selbtn.Name.Contains("c")) && (offest != 0) && (!new Rect(0, r1.Top, Width, 61).IntersectsWith(getrect(selbtn))))
                                || ((selbtn.Name.Contains("c")) && (offest == 0) && (!new Rect(r1.Left, 0, 61, Height).IntersectsWith(getrect(selbtn))))
                                || ((selbtn.Name.Contains("c")) && (!istouchwithbtns(r1)) && (offest == 0) && (itoeachfrompoint(new Rect(r1.Left + 30, r1.Top < selbtn.Margin.Top ? r1.Top : selbtn.Margin.Top + 30, 1, Math.Abs(selbtn.Margin.Top - r1.Top))) != 1)) //有一个目标但没有碰到
                                || ((selbtn.Name.Contains("c")) && (!istouchwithbtns(r1)) && (offest != 0) && (itoeachfrompoint(new Rect(r1.Left < selbtn.Margin.Left ? r1.Left : selbtn.Margin.Left + 30, r1.Top + 30, Math.Abs(r1.Left - selbtn.Margin.Left), 1)) != 1))
                                || ((selbtn.Name.Contains("p")) && (offest != 0) && (!new Rect(0, r1.Top, Width, 61).IntersectsWith(getrect(selbtn))))
                                || ((selbtn.Name.Contains("p")) && (offest == 0) && (!new Rect(r1.Left, 0, 61, Height).IntersectsWith(getrect(selbtn))))
                                || ((selbtn.Name.Contains("p")) && (istouchwithbtns(r1)) && (offest == 0) && (itoeachfrompoint(new Rect(r1.Left, r1.Top < selbtn.Margin.Top ? r1.Top : selbtn.Margin.Top + 30, 61, Math.Abs(selbtn.Margin.Top - r1.Top))) != 3))
                                || ((selbtn.Name.Contains("p")) && (istouchwithbtns(r1)) && (offest != 0) && (itoeachfrompoint(new Rect(r1.Left < selbtn.Margin.Left ? r1.Left : selbtn.Margin.Left + 30, r1.Top, Math.Abs(r1.Left - selbtn.Margin.Left), 61)) != 3))
                                || ((selbtn.Name.Contains("p")) && (!istouchwithbtns(r1)) && (offest == 0) && (itoeachfrompoint(new Rect(r1.Left + 30, r1.Top < selbtn.Margin.Top ? r1.Top : selbtn.Margin.Top + 30, 1, Math.Abs(selbtn.Margin.Top - r1.Top))) != 1)) //有一个目标但没有碰到
                                || ((selbtn.Name.Contains("p")) && (!istouchwithbtns(r1)) && (offest != 0) && (itoeachfrompoint(new Rect(r1.Left < selbtn.Margin.Left ? r1.Left : selbtn.Margin.Left + 30, r1.Top + 30, Math.Abs(r1.Left - selbtn.Margin.Left), 1)) != 1))
                                || ((selbtn.Name.Contains("b")) && (offest != 0) && (isowner) && (selbtn.Margin.Top > 330))
                                || ((selbtn.Name.Contains("b")) && (offest != 0) && (!isowner) && (selbtn.Margin.Top < 330))
                                || ((selbtn.Name.Contains("b")) && (isowner) && (r1.Top - 30 > selbtn.Margin.Top))//!!!!
                                || ((selbtn.Name.Contains("b")) && (!isowner) && (r1.Top < selbtn.Margin.Top))
                                || ((selbtn.Name.Contains("b")) && (len > 90))
                                )
                            {
                                //selbtn.Name = "abcdefg"
                            }
                            else
                            {
                                selbtn.Margin = new Thickness(selbtn.Margin.Left + offest * 72, selbtn.Margin.Top, 0, 0);

                                if (r1.Top < selbtn.Margin.Top)
                                { //向上
                                    off = (r1.Top - selbtn.Margin.Top) / 70;
                                    offest = (int)Math.Round(off, 0);
                                    selbtn.Margin = new Thickness(selbtn.Margin.Left, selbtn.Margin.Top + offest * 70, 0, 0);
                                }
                                else if (r1.Top > selbtn.Margin.Top)
                                {//向下
                                    off = (r1.Top - selbtn.Margin.Top) / 70;
                                    offest = (int)Math.Round(off, 0);
                                    selbtn.Margin = new Thickness(selbtn.Margin.Left, selbtn.Margin.Top + offest * 70, 0, 0);
                                }
                            }

                            if (lastt == selbtn.Margin)
                            {
                                o_g = isowner ? 0 : 1;
                                return;
                            }
                            string tmpdelbtns = "";
                            bool isover = false;
                            //吃///先判断敌我
                            for (int i = qis.Count - 1; i > -1; i--)
                            {
                                Rect br1 = getrect(qis[i]);
                                Rect br2 = getrect(selbtn);
                                if (br1.IntersectsWith(br2) && qis[i].Visibility == Visibility.Visible && qis[i] != selbtn)
                                {//吃（隐藏）
                                 //判断有没有踩到队友
                                    if (((int)qis[i].Tag) < 16 && ((int)selbtn.Tag) > 15 || ((int)qis[i].Tag) > 15 && ((int)selbtn.Tag) < 16)
                                    {
                                        qis[i].Visibility = Visibility.Hidden;
                                        if (qis[i].Name.Contains("j"))
                                        {
                                            isover = true;
                                        }
                                        tmpdelbtns += tmpdelbtns == "" ? qis[i].Name : "," + qis[i].Name;
                                    }
                                    else
                                    {//踩到队友
                                    }
                                    //判断是不是帅，是否输嘞！

                                }
                            }
                            //button.tag:left,top@button.tag,,,,,,
                            //并设置 turnto
                            if (isowner)
                            {
                                ownerfight(selbtn.Name + ":" + selbtn.Margin.Left.ToString() + "," + selbtn.Margin.Top.ToString() + "@" + tmpdelbtns);
                            }
                            else
                            {
                                guestfight(selbtn.Name + ":" + selbtn.Margin.Left.ToString() + "," + selbtn.Margin.Top.ToString() + "@" + tmpdelbtns);
                            }
                            //释放selbtn
                            deltmpls();
                            if (isover)
                            {
                                wtftimer.Stop();
                                MessageBox.Show("恭喜你赢了！");
                                return;
                            }
                            //}
                        }
                    }
                }
                catch
                {
                    await Task.Delay(100);
                    goto ERROR1;
                }
            }
            else
            {
                deltmpls();
                return;
            }
        }

        public Button findbtnbyname(string name)
        {
            Button btn = new Button();
            foreach (Button b in qis)
            {
                if (b.Visibility == Visibility.Visible)
                {
                    if (b.Name == name)
                    {
                        btn = b;
                        break;
                    }
                }
            }
            return btn;
        }


        Button tmpnewbtn = null;
        Button tmpnewstylebtn = null;
        public void setbtnbystring(string str_)
        {
            string[] strs = str_.Split('@');
            string str = strs[0];
            string[] delbtns = strs[1].Split(',');
            string[] infos = str.Split(':');
            string id = infos[0];
            string[] mars = infos[1].Split(',');
            double lleft = double.Parse(mars[0]);
            double ttop = double.Parse(mars[1]);
            //MessageBox.Show(id);
            Button thisbtn = findbtnbyname(id);
            if (tmpnewstylebtn != null)
                tmpnewstylebtn.BorderThickness = new Thickness(1);
            thisbtn.BorderThickness = new Thickness(3);
            tmpnewstylebtn = thisbtn;
            if (tmpnewbtn != null)
                g1.Children.Remove(tmpnewbtn);
            Button newbtn = new Button();
            newbtn.HorizontalAlignment = HorizontalAlignment.Left;
            newbtn.VerticalAlignment = VerticalAlignment.Top;
            newbtn.Margin = thisbtn.Margin;
            newbtn.Width = 61;newbtn.Height = 61;
            newbtn.BorderThickness = new Thickness(2);
            newbtn.BorderBrush = Brushes.Red;
            newbtn.Background = Brushes.Transparent;
            tmpnewbtn = newbtn;
            g1.Children.Add(newbtn);

            thisbtn.Margin = new Thickness(lleft, ttop, 0, 0);
            for (int i = delbtns.Length - 1; i > -1; i--)
            {
                findbtnbyname(delbtns[i]).Visibility = Visibility.Hidden;
            }
            if (strs[1].Contains("j"))
            {
                wtftimer.Stop();
                MessageBox.Show("菜狗这都能输！");
                return;
            }
        }

        private void setbtn_Click(object sender, RoutedEventArgs e)
        {
            login lo = new login();
            lo.Title = "设置";
            lo.ltip.Content = "（！留空表示不修改！）";
            lo.login1.Content = "确认";

            lo.lyx.Content = "昵称";
            lo.lpas.Content = "密码";
            lo.lnick.Content = "头像";
            lo.tmplogin.Content = "浏览";

            lo.lyz.Visibility = Visibility.Hidden;
            lo.yz.Visibility = Visibility.Hidden;
            lo.remme.Visibility = Visibility.Hidden;
            lo.lrme.Visibility = Visibility.Hidden;
            lo.tpagain.Visibility = Visibility.Hidden;
            //lo.tmplogin.Visibility = Visibility.Hidden;
            //lo.login1.Visibility = Visibility.Hidden;

            lo.lnick.HorizontalAlignment = HorizontalAlignment.Left;
            lo.lnick.VerticalAlignment = VerticalAlignment;
            lo.nick.HorizontalAlignment = HorizontalAlignment.Left;
            lo.nick.VerticalAlignment = VerticalAlignment;
            lo.tmplogin.HorizontalAlignment = HorizontalAlignment.Left;
            lo.tmplogin.VerticalAlignment = VerticalAlignment;
            lo.nick.Margin = new Thickness(lo.zh.Margin.Left, lo.nick.Margin.Top, 0, 0);
            lo.lnick.Margin = new Thickness(lo.lyx.Margin.Left, lo.lnick.Margin.Top, 0, 0);
            lo.tmplogin.Margin = new Thickness(lo.lnick.Margin.Left + lo.lnick.Width + 5, lo.lnick.Margin.Top, 0, 0);
            lo.Loaded -= lo.Window_Loaded;
            lo.Closing -= lo.Window_Closing;
            lo.login1.Click -= lo.Button_Click;
            lo.tmplogin.Click -= lo.tmplogin_Click;

            lo.login1.Click += (a, b) =>
            {
                if (lo.nick.Text.Replace(" ", "") != "")
                {
                    FileStream fstream = new FileStream(lo.nick.Text, FileMode.Open, FileAccess.Read);
                    try
                    {
                        byte[] buffur = new byte[fstream.Length];
                        fstream.Read(buffur, 0, (int)fstream.Length);
                        JIDs.UpdateOneAsync(new BsonDocument { { "UID", tuser.GetElement("UID").Value.ToString() } }, Builders<BsonDocument>.Update.Set("image", buffur));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        if (fstream != null)
                        {

                            //关闭资源  
                            fstream.Close();
                        }
                    }
                }
                try
                {
                    if (lo.pw.Text.Replace(" ", "") != "")
                    {
                        var update = Builders<BsonDocument>.Update.Set("password", lo.pw.Text.Replace(" ", ""));
                        JIDs.UpdateOneAsync(new BsonDocument { { "UID", tuser.GetElement("UID").Value.ToString() }, { "password", tuser.GetElement("password").Value.ToString() } }, update);
                    }
                    if (lo.zh.Text.Replace(" ", "") != "")//nick
                    {
                        var update = Builders<BsonDocument>.Update.Set("nick", lo.zh.Text.Replace(" ", ""));
                        JIDs.UpdateOneAsync(new BsonDocument { { "UID", tuser.GetElement("UID").Value.ToString() } }, update);
                    }
                    MessageBox.Show("重启后生效。");
                }
                catch { }
                lo.Close();
            };
            lo.tmplogin.Click += (a, b) =>
            {
                //创建一个打开文件式的对话框
                OpenFileDialog ofd = new OpenFileDialog();
                //设置这个对话框的起始打开路径
                //ofd.InitialDirectory = @"D:\";
                //设置打开的文件的类型，注意过滤器的语法
                ofd.Filter = "PNG图片|*.png|JPG图片|*.jpg|JPEG图片|*.jpeg";
                //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
                if (ofd.ShowDialog() == true)
                {
                    //image1.Source = new BitmapImage(new Uri(ofd.FileName));
                    lo.nick.Text = ofd.FileName;
                }
            };
            lo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            lo.Owner = this;
            lo.ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string m = "1.鼠标左键单击己方棋子，出现白色十字架时，移动到想走的地方，再单机，完成移动。\n" +
                "2.本棋不允许飞将。\n" +
                "3.注册登入后在左下角第1个按钮更改默认头像。\n" +
                "4.本棋将军时不发警报，不强制走棋，开发更多可能性。\n" +
                "5.注册登入后在左下角第4个按钮加好友，在对局中也可点击爱心加好友。\n" +
                "6.双击左侧的聊天记录可以复制内容。\n" +
                "7.本游戏全由B站杰尼龟梦回(QQ:329125460)独立开发。";
            MessageBox.Show(m);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                addf af = new addf();
                af.s.Click += async (a, b) =>
                {
                    if (af.str.Text.Replace(" ", "") != "")
                    {

                        var fbyyx = (await JIDs.FindAsync(new BsonDocument { { "UID", af.str.Text } })).ToList();
                        var fbynick = (await JIDs.FindAsync(new BsonDocument { { "nick", af.str.Text } })).ToList();
                        fbyyx.AddRange(fbynick);
                        af.lfriend.Items.Clear();
                        foreach (var i in fbyyx)
                        {
                            ListBoxItem item = new ListBoxItem();
                            Grid gitem = new Grid();
                            Label litem = new Label();
                            litem.Content = i.GetElement("nick").Value.ToString() + "(" + i.GetElement("UID").Value.ToString() + ")";
                            Image iitem = new Image();
                            iitem.VerticalAlignment = VerticalAlignment.Top;
                            iitem.HorizontalAlignment = HorizontalAlignment.Left;
                            litem.VerticalContentAlignment = VerticalAlignment.Center;
                            litem.VerticalAlignment = VerticalAlignment.Top;
                            litem.HorizontalAlignment = HorizontalAlignment.Left;
                            iitem.Margin = new Thickness(0);
                            litem.Margin = new Thickness(45, 0, 0, 0);
                            litem.Height = 40;
                            litem.FontSize = 14;
                            iitem.Width = 40; iitem.Height = 40;
                            item.Height = 40;
                            if (i.GetElement("image").Value.ToString().Replace(" ", "") == "")
                                iitem.Source = new BitmapImage(new Uri("images/棋子3(chessman3)_爱给网_aigei_com_01.png", UriKind.Relative));
                            else
                                iitem.Source = ToImage((byte[])i.GetElement("image").Value);
                            item.Padding = new Thickness(0);
                            gitem.Children.Add(iitem);
                            gitem.Children.Add(litem);
                            item.Content = gitem;

                            ContextMenu cMenu = new ContextMenu();
                            MenuItem menuItem = new MenuItem();
                            menuItem.Header = "加好友";
                            menuItem.Click += async (a1, b1) =>
                            {
                                var me = (await JIDs.FindAsync(new BsonDocument { { "UID", tuser.GetElement("UID").Value.ToString() } })).ToList()[0];
                                await JIDs.UpdateOneAsync(new BsonDocument { { "UID", tuser.GetElement("UID").Value.ToString() } }, Builders<BsonDocument>.Update.Set("friends", me.GetElement("friends").Value.ToString() + "\n" + i.GetElement("UID").Value.ToString()));
                                await JIDs.UpdateOneAsync(i, Builders<BsonDocument>.Update.Set("friends", i.GetElement("friends").Value.ToString() + "\n" + me.GetElement("UID").Value.ToString()));
                            };
                            cMenu.Items.Add(menuItem);
                            item.ContextMenu = cMenu;
                        /*item.MouseRightButtonDown += (a, b) =>
                        {
                            lastditem = item;
                        };
                        item.MouseRightButtonUp += (a, b) =>
                        {
                            if (lastditem == item)
                            {//显示friend右键菜单，否则不处理

                            }
                        };*/
                            af.lfriend.Items.Add(item);
                        }

                    }
                };
                af.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                af.Owner = this;
                af.ShowDialog();
            }
            catch { }
        }

        public static BitmapImage ToImage(byte[] byteArray)
        {
            BitmapImage bmp = null;

            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                bmp = null;
            }

            return bmp;
        }

        public void setowneringtmage()
        {
            ownername.Content = tuser.GetElement("nick").Value.ToString();
            BitmapImage image = null;
            try
            {
                image = ToImage((byte[])tuser.GetElement("image").Value);
            }
            catch { }
            if (image == null)
                return;
            ownert.Source = image;
        }

        public void setguestingtmage(BsonDocument bs)
        {
            guestname.Content = bs.GetElement("nick").Value.ToString();
            BitmapImage image = null;
            try
            {
                image = ToImage((byte[])bs.GetElement("image").Value);
            }
            catch { }
            if (image == null)
                return;
            guestt.Source = image;
        }

        ListBoxItem lastditem = new ListBoxItem();
        private async void myfriend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gfriend.Visibility == Visibility.Hidden)
                {
                    gfriend.Visibility = Visibility.Visible;
                    lfriend.Items.Clear();
                    string friendsstr = (await JIDs.FindAsync(new BsonDocument { { "UID", tuser.GetElement("UID").Value.ToString() } })).ToList()[0].GetElement("friends").Value.ToString();
                    string[] friends = friendsstr.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in friends)
                    {
                        ListBoxItem item = new ListBoxItem();
                        Grid gitem = new Grid();
                        var fuser = (await JIDs.FindAsync(new BsonDocument { { "UID", str } })).ToList();
                        if (fuser.Count == 0)
                            continue;
                        Label litem = new Label();
                        litem.Content = fuser[0].GetElement("nick").Value.ToString() + "(" + str + ")";
                        Image iitem = new Image();
                        iitem.VerticalAlignment = VerticalAlignment.Top;
                        iitem.HorizontalAlignment = HorizontalAlignment.Left;
                        litem.VerticalAlignment = VerticalAlignment.Top;
                        litem.VerticalContentAlignment = VerticalAlignment.Center;
                        litem.HorizontalAlignment = HorizontalAlignment.Left;
                        iitem.Margin = new Thickness(0);
                        litem.Margin = new Thickness(45, 0, 0, 0);
                        litem.Height = 40;
                        litem.FontSize = 14;
                        iitem.Width = 40; iitem.Height = 40;
                        item.Height = 40;
                        if (fuser[0].GetElement("image").Value.ToString().Replace(" ", "") == "")
                            iitem.Source = new BitmapImage(new Uri("images/棋子3(chessman3)_爱给网_aigei_com_01.png", UriKind.Relative));
                        else
                            iitem.Source = ToImage((byte[])fuser[0].GetElement("image").Value);
                        item.Padding = new Thickness(0);
                        gitem.Children.Add(iitem);
                        gitem.Children.Add(litem);
                        item.Content = gitem;

                        ContextMenu cMenu = new ContextMenu();
                        MenuItem menuItem = new MenuItem();
                        menuItem.Header = "来一把";
                        menuItem.Click += (a, b) =>
                        {//进入等待
                            ashostgetUID2 = str;
                            ashosttitle = tuser.GetElement("nick").Value.ToString() + "(" + tuser.GetElement("UID").Value.ToString() + ")";
                            ashostbody = "我想邀请你一起玩♂游戏行吗?";
                            ashosttype = "1";
                            isashostgetother = true;
                            Button_Click(null, null);
                        };
                        cMenu.Items.Add(menuItem);
                        item.ContextMenu = cMenu;
                        /*item.MouseRightButtonDown += (a, b) =>
                        {
                            lastditem = item;
                        };
                        item.MouseRightButtonUp += (a, b) =>
                        {
                            if (lastditem == item)
                            {//显示friend右键菜单，否则不处理

                            }
                        };*/
                        lfriend.Items.Add(item);
                    }
                }
                else
                {
                    gfriend.Visibility = Visibility.Hidden;
                }
            }
            catch { }
        }
    }
}
