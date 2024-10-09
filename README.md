# 精确拾取

1.允许使用鼠标右键拾取掉落物，可以设置拾取条件：右手为空，右手或左手为空，无条件。
2.会显示准星处的掉落物的名称与介绍，可以关闭介绍显示。
3.可以开关掉落物合并功能，允许设置掉落物合并的时间间隔与范围。
4.可以关闭掉落物自动拾取，只能使用右键拾取。
5.可以不允许在掉落物的位置放置方块。

配置文件：
`VintagestoryData/ModConfig/PrecisePickedUp.json`
```
{
  "PickupConditions": 0 // 右键拾取条件 0: 右手为空  1: 右手或左手为空  2: 无条件
  "CanAutoCollect": true, // 是否允许自动拾取掉落物
  "CanPlaceBlock": true, // 是否允许在掉落物处放置方块
  "ShowItemDescription": true, // 显示物品介绍
  "AutoMerge": true, // 掉落物自动合并
  "MergeRange": { // 合并范围
    "X": 1.0, // 水平范围
    "Y": 0.2 // 垂直范围
  },
  "MergeInterval": 3.0 // 合并间隔
}
```
