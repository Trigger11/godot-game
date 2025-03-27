using Godot;
using System;

/// <summary>
/// 放置区域类，用于检测卡牌是否可以放置在该区域
/// </summary>
public partial class DropZone : Control
{
    // 感应区域大小
    private Vector2 _sensorSize;
    /// <summary>
    /// 感应区域的大小
    /// </summary>
    public Vector2 SensorSize
    {
        get => _sensorSize;
        set
        {
            _sensorSize = value;
            if (_sensor != null)
            {
                _sensor.Size = value;
            }
        }
    }

    // 感应区域位置
    private Vector2 _sensorPosition;
    /// <summary>
    /// 感应区域的位置
    /// </summary>
    public Vector2 SensorPosition
    {
        get => _sensorPosition;
        set
        {
            _sensorPosition = value;
            if (_sensor != null)
            {
                _sensor.Position = value;
            }
        }
    }

    // 感应区域纹理
    private Texture2D _sensorTexture;
    /// <summary>
    /// 感应区域的纹理
    /// </summary>
    public Texture2D SensorTexture
    {
        get => _sensorTexture;
        set
        {
            _sensorTexture = value;
            if (_sensorHolder != null)
            {
                ((TextureRect)_sensorHolder).Texture = value;
            }
        }
    }

    // 感应区域可见性
    private bool _sensorVisible = true;
    /// <summary>
    /// 感应区域是否可见
    /// </summary>
    public bool SensorVisible
    {
        get => _sensorVisible;
        set
        {
            _sensorVisible = value;
            if (_sensor != null)
            {
                _sensor.Visible = value;
            }
        }
    }

    /// <summary>
    /// 存储的感应区域原始位置
    /// </summary>
    public Vector2 StoredSensorPosition { get; private set; }
    
    /// <summary>
    /// 父卡牌容器
    /// </summary>
    public CardContainer ParentCardContainer { get; set; }
    
    // 感应区域节点
    private Control _sensor;
    
    // 感应区域显示节点
    private Control _sensorHolder;

    /// <summary>
    /// 检查鼠标是否在放置区域内
    /// </summary>
    /// <returns>如果鼠标在放置区域内则返回true，否则返回false</returns>
    public bool CheckMouseIsInDropZone()
    {
        Vector2 mousePosition = GetGlobalMousePosition();
        var result = _sensor.GetGlobalRect().HasPoint(mousePosition);
        return result;
    }

    /// <summary>
    /// 设置感应区域的属性
    /// </summary>
    /// <param name="size">感应区域大小</param>
    /// <param name="position">感应区域位置</param>
    /// <param name="texture">感应区域纹理</param>
    /// <param name="visible">感应区域是否可见</param>
    public void SetSensor(Vector2 size, Vector2 position, Texture2D texture, bool visible)
    {
        // 创建感应区域
        if (_sensor == null)
        {
            _sensor = new TextureRect();
            _sensor.Name = "Sensor";
            _sensor.MouseFilter = MouseFilterEnum.Ignore;
            _sensor.ZIndex = -1000;
            AddChild(_sensor);
        }

        // 创建感应区域显示节点
        if (_sensorHolder == null)
        {
            _sensorHolder = new TextureRect();
            _sensorHolder.Name = "SensorHolder";
            _sensorHolder.MouseFilter = MouseFilterEnum.Ignore;
            _sensorHolder.ZIndex = -1000;
            AddChild(_sensorHolder);
            ((TextureRect)_sensorHolder).Size = size;
            _sensorHolder.Position = position;
            ((TextureRect)_sensorHolder).Texture = texture;
            _sensorHolder.Visible = visible;
        }

        // 设置属性
        SensorSize = size;
        SensorPosition = position;
        StoredSensorPosition = position;
        SensorVisible = visible;
    }

    /// <summary>
    /// 使用偏移量改变感应区域位置
    /// </summary>
    /// <param name="offset">位置偏移量</param>
    public void ChangeSensorPositionWithOffset(Vector2 offset)
    {
        SensorPosition = StoredSensorPosition + offset;
    }
}