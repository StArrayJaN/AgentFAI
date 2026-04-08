using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ADOFAI;
using UnityEngine;

namespace AgentFAI
{
    /// <summary>
    /// 关卡编辑工具方法集合
    /// 提供地板和事件的创建、删除、修改功能
    /// </summary>
    public static class LevelEditingTools
    {
        #region 地板操作

        [Description("在指定位置创建新地板")]
        public static bool CreateFloor(
            [Description("插入位置的地板索引")] int position,
            [Description("地板角度(度)，0=右，90=上，180=左，270=下")] float angle)
        {
            try
            {
                if (!ValidateFloorPosition(position))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 将角度转换为内部格式
                float internalAngle = ConvertAngleToInternal(angle);
                
                // 插入地板数据
                if (editor.GetField<bool>("isOldLevel"))
                {
                    // 旧版本：转换为字符并插入
                    char floorChar = ConvertAngleToChar(angle);
                    InsertFloorChar(position, floorChar);
                }
                else
                {
                    // 新版本：直接插入角度
                    InsertFloorAngle(position, internalAngle);
                }

                // 更新事件索引
                UpdateEventIndicesAfterInsert(position);
                
                // 重新生成关卡
                editor.ApplyEventsToFloors();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"CreateFloor failed: {ex.Message}");
                return false;
            }
        }

        [Description("删除指定位置的地板")]
        public static bool DeleteFloor(
            [Description("要删除的地板索引")] int floorIndex)
        {
            try
            {
                if (!ValidateFloorIndex(floorIndex))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 不能删除第一个地板
                if (floorIndex == 0) return false;

                // 删除地板数据
                if (editor.GetField<bool>("isOldLevel"))
                {
                    RemoveFloorChar(floorIndex);
                }
                else
                {
                    RemoveFloorAngle(floorIndex);
                }

                // 更新事件索引
                UpdateEventIndicesAfterDelete(floorIndex);
                
                // 重新生成关卡
                editor.ApplyEventsToFloors();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"DeleteFloor failed: {ex.Message}");
                return false;
            }
        }

        [Description("修改地板的角度")]
        public static bool ModifyFloorAngle(
            [Description("地板索引")] int floorIndex,
            [Description("新的角度(度)")] float newAngle)
        {
            try
            {
                if (!ValidateFloorIndex(floorIndex))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 修改地板数据
                if (editor.GetField<bool>("isOldLevel"))
                {
                    char newChar = ConvertAngleToChar(newAngle);
                    ModifyFloorChar(floorIndex, newChar);
                }
                else
                {
                    float internalAngle = ConvertAngleToInternal(newAngle);
                    ModifyFloorAngleData(floorIndex, internalAngle);
                }

                // 重新生成关卡
                editor.ApplyEventsToFloors();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ModifyFloorAngle failed: {ex.Message}");
                return false;
            }
        }

        [Description("获取关卡的地板总数")]
        public static int GetFloorCount()
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return 0;

                if (editor.GetField<bool>("isOldLevel"))
                {
                    return editor.levelData.pathData?.Length + 1 ?? 0;
                }
                else
                {
                    return editor.levelData.angleData?.Count + 1 ?? 0;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetFloorCount failed: {ex.Message}");
                return 0;
            }
        }

        [Description("获取指定地板的角度")]
        public static float GetFloorAngle(
            [Description("地板索引")] int floorIndex)
        {
            try
            {
                if (!ValidateFloorIndex(floorIndex))
                    return -1f;

                var editor = scnEditor.instance;
                if (editor == null) return -1f;

                // 第一个地板固定为270度(下)
                if (floorIndex == 0) return 270f;

                int dataIndex = floorIndex - 1;

                if (editor.GetField<bool>("isOldLevel"))
                {
                    if (dataIndex >= editor.levelData.pathData.Length)
                        return -1f;
                    
                    char floorChar = editor.levelData.pathData[dataIndex];
                    return ConvertCharToAngle(floorChar);
                }
                else
                {
                    if (dataIndex >= editor.levelData.angleData.Count)
                        return -1f;
                    
                    float internalAngle = editor.levelData.angleData[dataIndex];
                    return ConvertInternalToAngle(internalAngle);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetFloorAngle failed: {ex.Message}");
                return -1f;
            }
        }

        #endregion

        #region 事件操作

        [Description("为关卡添加事件，返回true为添加成功")]
        public static bool AddEvent(
            [Description("要添加事件的地板索引")] int floor,
            [Description("事件类型名称")] string eventType)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return false;

                if (!ValidateEventType(eventType))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return false;

                // 创建新事件
                var newEvent = new LevelEvent(floor, eventTypeEnum);
                editor.levelData.levelEvents.Add(newEvent);

                // 应用到关卡
                editor.ApplyEventsToFloors();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"AddEvent failed: {ex.Message}");
                return false;
            }
        }

        [Description("为关卡添加事件并设置属性")]
        public static bool AddEventWithProperties(
            [Description("要添加事件的地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("事件属性字典")] Dictionary<string, object> properties)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return false;

                if (!ValidateEventType(eventType))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return false;

                // 创建新事件
                var newEvent = new LevelEvent(floor, eventTypeEnum);

                // 设置属性
                if (properties != null)
                {
                    foreach (var kvp in properties)
                    {
                        if (newEvent.info.propertiesInfo.ContainsKey(kvp.Key))
                        {
                            var propertyInfo = newEvent.info.propertiesInfo[kvp.Key];
                            object validatedValue = ValidatePropertyValue(propertyInfo, kvp.Value);
                            if (validatedValue != null)
                            {
                                newEvent[kvp.Key] = validatedValue;
                            }
                        }
                    }
                }

                editor.levelData.levelEvents.Add(newEvent);

                // 应用到关卡
                editor.ApplyEventsToFloors();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"AddEventWithProperties failed: {ex.Message}");
                return false;
            }
        }

        [Description("为关卡添加事件并设置单个属性")]
        public static bool AddEventWithProperty(
            [Description("要添加事件的地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性名称")] string propertyName,
            [Description("属性值")] object propertyValue)
        {
            var properties = new Dictionary<string, object> { { propertyName, propertyValue } };
            return AddEventWithProperties(floor, eventType, properties);
        }

        [Description("删除指定地板上的事件")]
        public static bool RemoveEvent(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("事件索引(同类型事件中的第几个，从0开始)")] int eventIndex = 0)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return false;

                if (!ValidateEventType(eventType))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return false;

                // 查找指定类型的所有事件
                var eventsOfType = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == eventTypeEnum)
                    .ToList();

                if (eventIndex >= 0 && eventIndex < eventsOfType.Count)
                {
                    var eventToRemove = eventsOfType[eventIndex];
                    editor.levelData.levelEvents.Remove(eventToRemove);
                    editor.ApplyEventsToFloors();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"RemoveEvent failed: {ex.Message}");
                return false;
            }
        }

        [Description("删除指定地板上匹配属性条件的事件")]
        public static int RemoveEventsByProperty(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性名称")] string propertyName,
            [Description("属性值")] object propertyValue)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return 0;

                if (!ValidateEventType(eventType))
                    return 0;

                var editor = scnEditor.instance;
                if (editor == null) return 0;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return 0;

                // 查找匹配条件的事件
                var eventsToRemove = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && 
                               e.eventType == eventTypeEnum &&
                               EventPropertyMatches(e, propertyName, propertyValue))
                    .ToList();

                int removedCount = eventsToRemove.Count;
                foreach (var evt in eventsToRemove)
                {
                    editor.levelData.levelEvents.Remove(evt);
                }

                if (removedCount > 0)
                {
                    editor.ApplyEventsToFloors();
                }

                return removedCount;
            }
            catch (Exception ex)
            {
                Debug.LogError($"RemoveEventsByProperty failed: {ex.Message}");
                return 0;
            }
        }

        [Description("删除指定地板上匹配多个属性条件的事件")]
        public static int RemoveEventsByProperties(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性条件字典")] Dictionary<string, object> propertyConditions)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return 0;

                if (!ValidateEventType(eventType))
                    return 0;

                var editor = scnEditor.instance;
                if (editor == null) return 0;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return 0;

                // 查找匹配所有条件的事件
                var eventsToRemove = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && 
                               e.eventType == eventTypeEnum &&
                               EventPropertiesMatch(e, propertyConditions))
                    .ToList();

                int removedCount = eventsToRemove.Count;
                foreach (var evt in eventsToRemove)
                {
                    editor.levelData.levelEvents.Remove(evt);
                }

                if (removedCount > 0)
                {
                    editor.ApplyEventsToFloors();
                }

                return removedCount;
            }
            catch (Exception ex)
            {
                Debug.LogError($"RemoveEventsByProperties failed: {ex.Message}");
                return 0;
            }
        }

        [Description("删除指定地板上指定类型的所有事件")]
        public static int RemoveAllEvents(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return 0;

                if (!ValidateEventType(eventType))
                    return 0;

                var editor = scnEditor.instance;
                if (editor == null) return 0;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return 0;

                // 查找并删除所有指定类型的事件
                var eventsToRemove = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == eventTypeEnum)
                    .ToList();

                int removedCount = eventsToRemove.Count;
                foreach (var evt in eventsToRemove)
                {
                    editor.levelData.levelEvents.Remove(evt);
                }

                if (removedCount > 0)
                {
                    editor.ApplyEventsToFloors();
                }

                return removedCount;
            }
            catch (Exception ex)
            {
                Debug.LogError($"RemoveAllEvents failed: {ex.Message}");
                return 0;
            }
        }

        [Description("修改事件的属性值")]
        public static bool SetEventProperty(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性名称")] string propertyName,
            [Description("属性值")] object value,
            [Description("事件索引(同类型事件中的第几个，从0开始)")] int eventIndex = 0)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return false;

                var editor = scnEditor.instance;
                if (editor == null) return false;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return false;

                // 查找指定索引的事件
                var eventsOfType = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == eventTypeEnum)
                    .ToList();

                if (eventIndex < 0 || eventIndex >= eventsOfType.Count)
                    return false;

                var targetEvent = eventsOfType[eventIndex];

                // 验证属性
                if (!targetEvent.info.propertiesInfo.ContainsKey(propertyName))
                    return false;

                var propertyInfo = targetEvent.info.propertiesInfo[propertyName];
                
                // 验证并设置属性值
                object validatedValue = ValidatePropertyValue(propertyInfo, value);
                if (validatedValue == null) return false;

                targetEvent[propertyName] = validatedValue;
                
                // 应用到关卡
                editor.ApplyEventsToFloors();
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SetEventProperty failed: {ex.Message}");
                return false;
            }
        }

        [Description("批量修改同类型事件的属性值")]
        public static int SetAllEventProperties(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性名称")] string propertyName,
            [Description("属性值")] object value)
        {
            try
            {
                if (!ValidateFloorIndex(floor))
                    return 0;

                var editor = scnEditor.instance;
                if (editor == null) return 0;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return 0;

                // 查找所有指定类型的事件
                var eventsOfType = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == eventTypeEnum)
                    .ToList();

                int modifiedCount = 0;
                foreach (var targetEvent in eventsOfType)
                {
                    // 验证属性
                    if (!targetEvent.info.propertiesInfo.ContainsKey(propertyName))
                        continue;

                    var propertyInfo = targetEvent.info.propertiesInfo[propertyName];
                    
                    // 验证并设置属性值
                    object validatedValue = ValidatePropertyValue(propertyInfo, value);
                    if (validatedValue != null)
                    {
                        targetEvent[propertyName] = validatedValue;
                        modifiedCount++;
                    }
                }

                if (modifiedCount > 0)
                {
                    // 应用到关卡
                    editor.ApplyEventsToFloors();
                }
                
                return modifiedCount;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SetAllEventProperties failed: {ex.Message}");
                return 0;
            }
        }

        [Description("获取事件的属性值")]
        public static object GetEventProperty(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性名称")] string propertyName,
            [Description("事件索引(同类型事件中的第几个，从0开始)")] int eventIndex = 0)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return null;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return null;

                // 查找指定索引的事件
                var eventsOfType = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == eventTypeEnum)
                    .ToList();

                if (eventIndex < 0 || eventIndex >= eventsOfType.Count)
                    return null;

                var targetEvent = eventsOfType[eventIndex];

                // 获取属性值
                return targetEvent[propertyName];
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetEventProperty failed: {ex.Message}");
                return null;
            }
        }

        [Description("获取指定类型事件的数量")]
        public static int GetEventCount(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return 0;

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return 0;

                // 计算指定类型事件的数量
                return editor.levelData.levelEvents
                    .Count(e => e.floor == floor && e.eventType == eventTypeEnum);
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetEventCount failed: {ex.Message}");
                return 0;
            }
        }

        [Description("获取指定地板上的所有事件")]
        public static string[] GetEventsOnFloor(
            [Description("地板索引")] int floor)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return new string[0];

                var events = editor.levelData.levelEvents
                    .Where(e => e.floor == floor)
                    .Select(e => e.eventType.ToString())
                    .ToArray();

                return events;
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetEventsOnFloor failed: {ex.Message}");
                return new string[0];
            }
        }

        [Description("获取指定地板上的事件详细信息")]
        public static EventInfo[] GetEventDetailsOnFloor(
            [Description("地板索引")] int floor)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return new EventInfo[0];

                var events = editor.levelData.levelEvents
                    .Where(e => e.floor == floor)
                    .Select((e, index) => new EventInfo
                    {
                        EventType = e.eventType.ToString(),
                        Index = GetEventIndexInSameType(e, floor),
                        Active = e.active,
                        Visible = e.visible
                    })
                    .ToArray();

                return events;
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetEventDetailsOnFloor failed: {ex.Message}");
                return new EventInfo[0];
            }
        }

        [Description("获取指定地板上指定类型事件的所有属性")]
        public static Dictionary<string, object>[] GetAllEventProperties(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return new Dictionary<string, object>[0];

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return new Dictionary<string, object>[0];

                var eventsOfType = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == eventTypeEnum)
                    .ToList();

                var result = new Dictionary<string, object>[eventsOfType.Count];
                for (int i = 0; i < eventsOfType.Count; i++)
                {
                    result[i] = new Dictionary<string, object>(eventsOfType[i].data);
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetAllEventProperties failed: {ex.Message}");
                return new Dictionary<string, object>[0];
            }
        }

        [Description("查找匹配属性条件的事件")]
        public static EventInfo[] FindEventsByProperty(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性名称")] string propertyName,
            [Description("属性值")] object propertyValue)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return new EventInfo[0];

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return new EventInfo[0];

                var matchingEvents = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && 
                               e.eventType == eventTypeEnum &&
                               EventPropertyMatches(e, propertyName, propertyValue))
                    .Select((e, index) => new EventInfo
                    {
                        EventType = e.eventType.ToString(),
                        Index = GetEventIndexInSameType(e, floor),
                        Active = e.active,
                        Visible = e.visible
                    })
                    .ToArray();

                return matchingEvents;
            }
            catch (Exception ex)
            {
                Debug.LogError($"FindEventsByProperty failed: {ex.Message}");
                return new EventInfo[0];
            }
        }

        [Description("查找匹配多个属性条件的事件")]
        public static EventInfo[] FindEventsByProperties(
            [Description("地板索引")] int floor,
            [Description("事件类型名称")] string eventType,
            [Description("属性条件字典")] Dictionary<string, object> propertyConditions)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return new EventInfo[0];

                // 解析事件类型
                if (!Enum.TryParse<LevelEventType>(eventType, out var eventTypeEnum))
                    return new EventInfo[0];

                var matchingEvents = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && 
                               e.eventType == eventTypeEnum &&
                               EventPropertiesMatch(e, propertyConditions))
                    .Select((e, index) => new EventInfo
                    {
                        EventType = e.eventType.ToString(),
                        Index = GetEventIndexInSameType(e, floor),
                        Active = e.active,
                        Visible = e.visible
                    })
                    .ToArray();

                return matchingEvents;
            }
            catch (Exception ex)
            {
                Debug.LogError($"FindEventsByProperties failed: {ex.Message}");
                return new EventInfo[0];
            }
        }

        #endregion

        #region 关卡信息操作

        [Description("设置关卡基本信息")]
        public static bool SetLevelInfo(
            [Description("艺术家名称")] string artist,
            [Description("歌曲名称")] string song,
            [Description("作者名称")] string author,
            [Description("BPM值")] float bpm)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return false;

                var levelData = editor.levelData;
                
                levelData.artist = artist ?? "";
                levelData.levelSettings["song"] = song ?? "";
                levelData.levelSettings["author"] = author ?? "";
                levelData.songSettings["bpm"] = bpm;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SetLevelInfo failed: {ex.Message}");
                return false;
            }
        }

        [Description("获取关卡基本信息")]
        public static LevelInfo GetLevelInfo()
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return null;

                var levelData = editor.levelData;
                
                return new LevelInfo
                {
                    Artist = levelData.artist,
                    Song = levelData.song,
                    Author = levelData.author,
                    BPM = levelData.bpm
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetLevelInfo failed: {ex.Message}");
                return null;
            }
        }

        [Description("保存当前关卡")]
        public static bool SaveLevel(
            [Description("保存路径")] string filePath)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return false;

                string levelJson = editor.levelData.Encode();
                System.IO.File.WriteAllText(filePath, levelJson);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveLevel failed: {ex.Message}");
                return false;
            }
        }

        [Description("加载关卡文件")]
        public static bool LoadLevel(
            [Description("关卡文件路径")] string filePath)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return false;

                if (!System.IO.File.Exists(filePath))
                    return false;

                editor.OpenLevel(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"LoadLevel failed: {ex}");
                return false;
            }
        }

        #endregion

        #region 辅助方法

        private static bool ValidateFloorPosition(int position)
        {
            return position >= 0 && position <= GetFloorCount();
        }

        private static bool ValidateFloorIndex(int floorIndex)
        {
            return floorIndex >= 0 && floorIndex < GetFloorCount();
        }

        private static bool ValidateEventType(string eventType)
        {
            return Enum.TryParse<LevelEventType>(eventType, out _);
        }

        private static float ConvertAngleToInternal(float angle)
        {
            // 转换用户角度(0=右,90=上)到内部角度(0=上,90=右)
            return (90f - angle) % 360f;
        }

        private static float ConvertInternalToAngle(float internalAngle)
        {
            // 转换内部角度到用户角度
            return (90f - internalAngle) % 360f;
        }

        private static char ConvertAngleToChar(float angle)
        {
            // 角度到字符的映射
            int normalizedAngle = Mathf.RoundToInt(angle) % 360;
            if (normalizedAngle < 0) normalizedAngle += 360;

            return normalizedAngle switch
            {
                0 => 'R',    // 右
                45 => 'E',   // 右上
                90 => 'U',   // 上
                135 => 'Q',  // 左上
                180 => 'L',  // 左
                225 => 'Z',  // 左下
                270 => 'D',  // 下
                315 => 'C',  // 右下
                _ => 'R'     // 默认右
            };
        }

        private static float ConvertCharToAngle(char floorChar)
        {
            return floorChar switch
            {
                'R' => 0f,    // 右
                'E' => 45f,   // 右上
                'U' => 90f,   // 上
                'Q' => 135f,  // 左上
                'L' => 180f,  // 左
                'Z' => 225f,  // 左下
                'D' => 270f,  // 下
                'C' => 315f,  // 右下
                '!' => 999f,  // 中旋
                _ => 0f       // 默认右
            };
        }

        private static void InsertFloorChar(int position, char floorChar)
        {
            var editor = scnEditor.instance;
            var pathData = editor.levelData.pathData;
            
            if (position >= pathData.Length)
            {
                editor.levelData.pathData = pathData + floorChar;
            }
            else
            {
                editor.levelData.pathData = pathData.Insert(position, floorChar.ToString());
            }
        }

        private static void InsertFloorAngle(int position, float angle)
        {
            var editor = scnEditor.instance;
            var angleData = editor.levelData.angleData;
            
            if (position >= angleData.Count)
            {
                angleData.Add(angle);
            }
            else
            {
                angleData.Insert(position, angle);
            }
        }

        private static void RemoveFloorChar(int index)
        {
            var editor = scnEditor.instance;
            var pathData = editor.levelData.pathData;
            
            if (index > 0 && index <= pathData.Length)
            {
                editor.levelData.pathData = pathData.Remove(index - 1, 1);
            }
        }

        private static void RemoveFloorAngle(int index)
        {
            var editor = scnEditor.instance;
            var angleData = editor.levelData.angleData;
            
            if (index > 0 && index <= angleData.Count)
            {
                angleData.RemoveAt(index - 1);
            }
        }

        private static void ModifyFloorChar(int index, char newChar)
        {
            var editor = scnEditor.instance;
            var pathData = editor.levelData.pathData;
            
            if (index > 0 && index <= pathData.Length)
            {
                var chars = pathData.ToCharArray();
                chars[index - 1] = newChar;
                editor.levelData.pathData = new string(chars);
            }
        }

        private static void ModifyFloorAngleData(int index, float newAngle)
        {
            var editor = scnEditor.instance;
            var angleData = editor.levelData.angleData;
            
            if (index > 0 && index <= angleData.Count)
            {
                angleData[index - 1] = newAngle;
            }
        }

        private static void UpdateEventIndicesAfterInsert(int insertPosition)
        {
            var editor = scnEditor.instance;
            foreach (var evt in editor.levelData.levelEvents)
            {
                if (evt.floor >= insertPosition)
                {
                    evt.floor++;
                }
            }
        }

        private static void UpdateEventIndicesAfterDelete(int deletePosition)
        {
            var editor = scnEditor.instance;
            foreach (var evt in editor.levelData.levelEvents)
            {
                if (evt.floor > deletePosition)
                {
                    evt.floor--;
                }
            }
        }

        private static object ValidatePropertyValue(PropertyInfo propertyInfo, object value)
        {
            try
            {
                switch (propertyInfo.type)
                {
                    case PropertyType.Int:
                        return propertyInfo.Validate(Convert.ToInt32(value));
                    case PropertyType.Float:
                        return propertyInfo.Validate(Convert.ToSingle(value));
                    case PropertyType.Bool:
                        return Convert.ToBoolean(value);
                    case PropertyType.String:
                        return value?.ToString() ?? "";
                    case PropertyType.Vector2:
                        if (value is Vector2 v2) return propertyInfo.Validate(v2);
                        break;
                    default:
                        return value;
                }
                return value;
            }
            catch
            {
                return null;
            }
        }

        private static bool EventPropertyMatches(LevelEvent evt, string propertyName, object expectedValue)
        {
            try
            {
                if (!evt.info.propertiesInfo.ContainsKey(propertyName))
                    return false;

                object actualValue = evt[propertyName];
                
                // 处理null值比较
                if (actualValue == null && expectedValue == null)
                    return true;
                if (actualValue == null || expectedValue == null)
                    return false;

                // 类型转换和比较
                if (actualValue.GetType() != expectedValue.GetType())
                {
                    // 尝试转换类型进行比较
                    try
                    {
                        if (actualValue is IConvertible && expectedValue is IConvertible)
                        {
                            var convertedExpected = Convert.ChangeType(expectedValue, actualValue.GetType());
                            return actualValue.Equals(convertedExpected);
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }

                return actualValue.Equals(expectedValue);
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventPropertyMatches failed: {ex.Message}");
                return false;
            }
        }

        private static bool EventPropertiesMatch(LevelEvent evt, Dictionary<string, object> propertyConditions)
        {
            try
            {
                if (propertyConditions == null || propertyConditions.Count == 0)
                    return true;

                foreach (var condition in propertyConditions)
                {
                    if (!EventPropertyMatches(evt, condition.Key, condition.Value))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"EventPropertiesMatch failed: {ex.Message}");
                return false;
            }
        }

        private static int GetEventIndexInSameType(LevelEvent targetEvent, int floor)
        {
            try
            {
                var editor = scnEditor.instance;
                if (editor == null) return -1;

                var eventsOfSameType = editor.levelData.levelEvents
                    .Where(e => e.floor == floor && e.eventType == targetEvent.eventType)
                    .ToList();

                return eventsOfSameType.IndexOf(targetEvent);
            }
            catch (Exception ex)
            {
                Debug.LogError($"GetEventIndexInSameType failed: {ex.Message}");
                return -1;
            }
        }

        #endregion
    }

    /// <summary>
    /// 关卡信息数据模型
    /// </summary>
    public class LevelInfo
    {
        public string Artist { get; set; }
        public string Song { get; set; }
        public string Author { get; set; }
        public float BPM { get; set; }
    }

    /// <summary>
    /// 事件信息数据模型
    /// </summary>
    public class EventInfo
    {
        public string EventType { get; set; }
        public int Index { get; set; }
        public bool Active { get; set; }
        public bool Visible { get; set; }
    }
}