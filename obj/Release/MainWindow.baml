   M S B A M L   `   `   `  ����   	ChessApp1    ChessApp1.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Шахматы��$	��650��$	��650�� � WindowStartupLocation$ CenterScreen=� � 
ResizeMode$ NoResize=� � 	Resources    RoundButtonTemplate �� 
TargetType ��Name �� CornerRadius �� Triggers P� Property	 P� Value
 �� Property �� Value��%�   &	        �� " ��.R��� $ border��#��z��#��z��#��z��$ 10��._��� $��Center=�$��Center=� P�  �@J�IsMouseOver	 . . ��� 
 � �� �	#FFE5E5E5.���  � ColumnDefinitions �� $
��383* ��� $
��267* � � RowDefinitions �� $
��Auto ��� $��* �.+��� $��0�� �� Orientation$ 
Horizontal=�$��10,10,10,10q�$��2��.+��� -    ��Name$ 	BotButton��$��Бот��$��60��$��30��$��0,0,10,0q�#��[  �� -    ��Name$ DifficultyComboBox��$	��100��$��30��.@��� $��Новичок���� $��Любитель���� $��Средний���� $��Сильный���� $��Мастер���� -    ��Name$ 
StatusText��$��20,0,0,0q�$��Center=�$��16(�� -    �Name$ 
ChessBoard��$��1��$��10,10,10,10q�$��2�� �� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* � �� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* ��� $��* �