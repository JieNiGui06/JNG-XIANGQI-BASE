   M S B A M L   `   `   `  ????G  C杰尼象棋, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:杰尼象棋杰尼象棋  5   	       杰尼象棋.login   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ,localclr-namespace:杰尼象棋     5   	   _ [MaterialDesignThemes.Wpf, Version=4.6.0.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5	   	   -   5          ? Title$  login??5   	   $	??166??6   $	??446??6$      MaterialDesignBody#???   5
   	   $??Regular"?5   	   $??13(?5   	   &   System.Windows.Media.TextOptions   TextFormattingMode$ Ideal=?5   	      TextRenderingMode$
 Auto=?5   	     MaterialDesignPaper#???  5   	     MaterialDesignFont#???  5   	    ? 
ResizeMode$ NoResize=?6;    ? SizeToContent$ Manual=?6Q    ? WindowStartupLocation$ Manual=?6h   .??5      ? -   5         ?Name$ g1??5      .+?5   
   ?? -   5         ??Name$
 ltip??5      $??JNG项目全部通用??6   $??23??62   $	??Top=?6>   $??10(?6V   $??Center=?6d   $??Center=?6?   $??0,0,0,0q?6?   ???	#FF00518F???	#2D03497E6?   ?? 5   
   -   5         ??Name$ login1??5      $??登入/注册??6!   $??0,0,17,9q?61   $??0,3,0,0q?6C   $??10(?6U   ???	#FF3A8EB7???	#FF101392$??23??6?   $??Bottom=?6?   $??Right=?6?   $??55??6?   $??Microsoft YaHei)?6?   ?? 5   
   -   5        $ tmplogin??5        MaterialDesignPaperButton#??[ 6#   $??游客登入??6V   $??Right=?6e   $??0,0,93,9q?6?   $??55??6?   $??0,3,0,0q?6?   $??10(?6?   $??23??6?   $??Bottom=?6?   $??Microsoft YaHei)?6?   6  ?? 5   
   -   5        $
 lpas??5      $??密码??6   $??23??6+   $	??Top=?67   $??10(?6O   $
??Left=?6]   $??Center=?6   $??15,0,0,0q?6?   ???	#FF00518F$??0,64,0,0q?6?   ??5      ?? 5      	 ?? EndPoint$		 1,1&?6&   
 ?? 
StartPoint$	
 0,0&?65   .??5      ?  ? Color$ 	#0C03497E??6#    ? Offset$ 1Q?65   ? 5      $ 	#2D03497E??6#   $ 0Q?65   5      5      ?? 5   
   -   5        $	 lyx??5      $??邮箱??6   $??23??6*   $	??Top=?66   $??10(?6N   $
??Left=?6\   $??Center=?6~   $??15,0,0,0q?6?   ???	#FF00518F$??0,35,0,0q?6?   ??5      ?? 5      $		 1,1&?6&   $	
 0,0&?65   .??5       ? $ 	#0C03497E??6#   $ 1Q?65   ? 5!      $ 	#2D03497E??6#   $ 0Q?65   5"      5$      ?? 5%   
   -   5        $	 lyz??5%      $??	验证码??6   $??10(?6+   $
??Left=?69   $??Center=?6[   $??15,0,0,0q?6}   ???	#FF00518F$??0,91,0,0q?6?   $??Right=?6?   $	??223??6?   $??23??6?   $	??Top=?6?   ??5&      ?? 5'      $		 1,1&?6&   $	
 0,0&?65   .??5(      ? $ 	#0C03497E??6#   $ 1Q?65   ? 5)      $ 	#2D03497E??6#   $ 0Q?65   5*      5,      ?? 5-   
   -	   5         ??Name$ zh??5-      $
??Left=?6   $??22??69   $??	70,36,0,0q?6E    ?? TextWrapping$
 Wrap=?6X   $	??Top=?6l   $	??376??6?   $??10(?6?   ???	#FF535F706?   ?? 5.   
   -
   5        $ pw??5.      $
??Left=?6   $??22??69   $??	70,64,0,0q?6E   $
 Wrap=?6X   $	??Top=?6l   $	??376??6?   $??10(?6?   ???	#FF535F706?   ?? 5/   
   -   5        $ yz??5/      $??Right=?6   $??22??6:   $??	0,91,44,0q?6F   $
 Wrap=?6Y   $	??Top=?6m   $	??112??6?   $??10(?6?   ???	#FF535F706?   ?? 50   
   -   5         ??Name$ remme??50      $??CheckBox??6"   $
??Left=?65   $??	15,96,0,0q?6P   $??17??6c   $??17??6n   $	??Top=?6z   ?? 51   
   -   5        $
 lrme??51      $??	记住我??6   $
??Left=?6,   $??	37,96,0,0q?6G   $??43??6Z   $??0,0,0,0q?6e   $
??Left=?6w   $??Center=?6?   $??8(?6?   ???	#A7000000$??17??6?   $	??Top=?6?   ?? 52   
   -   5        $ lnick??52      $??昵称??6   $??10(?6,   $
??Left=?6:   $??Center=?6\   $??15,0,0,0q?6~   ???	#FF00518F$??
0,91,223,0q?6?   $??Right=?6?   $	??156??6?   $??23??6?   $	??Top=?6?   ??53      ?? 54      $		 1,1&?6&   $	
 0,0&?65   .??55      ? $ 	#0C03497E??6#   $ 1Q?65   ? 56      $ 	#2D03497E??6#   $ 0Q?65   57      59      ?? 5:   
   -   5        $
 nick??5:      $??Right=?6    $??22??6<   $??
0,91,238,0q?6H   $
 Wrap=?6\   $	??Top=?6p   $??94??6?   $??10(?6?   ???	#FF535F706?   ?? 5;   
   -   5        $ tpagain??5;      #??[ 6"   $??重新发送??6U   $??Right=?6d   $??	0,0,168,9q?6?   $??55??6?   $??0,3,0,0q?6?   $??10(?6?   $??23??6?   $??Bottom=?6?   $??Microsoft YaHei)?6?   6  5=      5>      