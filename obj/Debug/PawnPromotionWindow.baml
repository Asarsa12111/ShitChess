   M S B A M L   `   `   `  ����  	ChessApp1#    ChessApp1.PawnPromotionWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$'  !Превращение пешки��5   	   $	��200��5   	   $	��400��5   	    � WindowStartupLocation$ CenterOwner=�5   	    � 
ResizeMode$ NoResize=�5	   	    � WindowStyle$ 
ToolWindow=�5
   	   .��5      �  � ColumnDefinitions 5   
   �� 5      $��9* �6   �� 5      $	��41* �6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5   
   �� $��0��6   $M��GВыберите фигуру для превращения пешки:��5      $��16(�5      $
��Left=�5      $��
41,10,0,10q�5      $��2��6(   � 5    
   $��1��6   $��10,10,10,10q�6   $��2��61    5"      �� 5#      $��* �6#   �� 5$      $��* �6#   �� 5%      $��* �6#   �� 5&      $��* �6#   .+�5*      �� -   5         ��Name$ QueenButton��5*      $��0��5+      $��5q�5,      5-      �� 50      -   5        $ 
RookButton��50      $��1��51      $��5q�52      53      �� 56      -   5        $ BishopButton��56      $��2��57      $��5q�58      59      �� 5<      -   5        $ KnightButton��5<      $��3��5=      $��5q�5>      5?      5@      5A      5B      