# 精确拾取

1. 允许使用鼠标右键拾取掉落物和投射物，可以设置拾取条件：右手为空，右手或左手为空，无条件。
2. 会显示准星处的掉落物和投射物的名称与介绍，可以关闭介绍显示。
3. 可以开关掉落物合并功能，允许设置掉落物合并的时间间隔与范围。
4. 可以关闭掉落物自动拾取，只能使用右键拾取。
5. 可以不允许在掉落物的位置放置方块。
6. 默认开启范围拾取功能，以交互的掉落物/投射物为中心，将拾取指定范围内的所有同种掉落物/投射物

配置文件：`VintagestoryData/ModConfig/PrecisePickedUp.json`
```
{
  // 客户端配置：
  "ShowItemDescription": true, // 显示物品介绍

  // 服务端配置：
  "PickupConditions": 1 // 右键拾取条件 0: 右手为空  1: 右手或左手为空  2: 无条件
  "CanAutoCollect": true, // 是否允许自动拾取掉落物
  "CanPlaceBlock": true, // 是否允许在掉落物处放置方块
  "AutoMerge": true, // 掉落物自动合并
  "MergeRange": { // 合并范围
    "X": 1.5, // 水平范围
    "Y": 0.2 // 垂直范围
  },
  "MergeInterval": 5.0, // 合并间隔
  "RangePickup": true, // 范围拾取
  "PickupRange": { // 拾取范围
    "X": 1.5, // 水平范围
    "Y": 0.2 // 垂直范围
  }
}
```

# Precise Pickup  

1. Allows using the right mouse button to pick up dropped items and projectiles, with configurable pickup conditions: right hand empty, either hand empty, or no conditions.  
2. Displays the name and description of the targeted dropped item or projectile. The description display can be toggled off.  
3. Enables/disables item merging for dropped items, with adjustable merge interval and range.  
4. Can disable automatic pickup of dropped items, restricting collection to right-click only.  
5. Option to prevent block placement at the location of dropped items.  
6. Range pickup is enabled by default, allowing pickup of all same-type dropped items/projectiles within a specified radius around the interacted item/projectile.  

Configuration file: `VintagestoryData/ModConfig/PrecisePickedUp.json`  
```
{
  // Client Side Config: 
  "ShowItemDescription": true, // Show item descriptions  

  // Server Side Config: 
  "PickupConditions": 1, // Right-click pickup conditions: 0: Right hand empty, 1: Either hand empty, 2: No conditions  
  "CanAutoCollect": true, // Enable automatic pickup of dropped items  
  "CanPlaceBlock": true, // Allow block placement at dropped item locations  
  "AutoMerge": true, // Enable automatic merging of dropped items  
  "MergeRange": { // Merge range  
    "X": 1.5, // Horizontal range  
    "Y": 0.2 // Vertical range  
  },  
  "MergeInterval": 5.0, // Merge interval (seconds)  
  "RangePickup": true, // Enable range pickup  
  "PickupRange": { // Pickup range  
    "X": 1.5, // Horizontal range  
    "Y": 0.2 // Vertical range  
  }  
}  
```
