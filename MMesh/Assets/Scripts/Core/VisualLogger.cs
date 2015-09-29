using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public interface ILogger
{
    void Update();
    void Log(string message);
    void Log(string message, Color color);
}

public class VisualLogger : ILogger
{
	private List<VisualLog> visualLogs;

    public VisualLogger()
	{
		visualLogs = new List<VisualLog>();
	}

	public void Update()
	{

        if((visualLogs.Count+1) * 20 > Screen.height)
            visualLogs.RemoveAt(0);

		for(int log = visualLogs.Count-1;log>=0;log--)
		{
            // Draw shadow
            GUI.color = new Color(0f,0f,0f,visualLogs[log].Color.a);
            GUI.Label(new Rect(10, (log * 20) + 1, 600, 20), visualLogs[log].Message);
            GUI.Label(new Rect(10, (log * 20) + 2, 600, 20), visualLogs[log].Message);
            // Draw text
			GUI.color = visualLogs[log].Color;
			GUI.Label(new Rect(10, log * 20, 600, 20), visualLogs[log].Message);

			visualLogs[log].LifeTime-=Time.deltaTime;

			if(visualLogs[log].LifeTime <= 0.0f)
				visualLogs.RemoveAt(log);
		}
	}

	public void Log( string message )
	{
		visualLogs.Add(new VisualLog(message));
	}

    public void Log(string message, Color color)
    {
        visualLogs.Add(new VisualLog(message, color));
    }
}

public class VisualLog
{
	private string message;
	public string Message {
		get {
			return message;
		}
	}

	private Color color;
	public Color Color {
		get {
			return color;
		}
	}

	private const float VISUALLOG_LIFETIME = 40.0f;
    private float lifeTime;
    public float LifeTime
    {
        get
        {
            if (lifeTime < 2f)
                color.a = lifeTime * 0.5f;

            if (lifeTime > VISUALLOG_LIFETIME - 0.5f)
                color.a = ((VISUALLOG_LIFETIME - lifeTime) * 2f);
            return lifeTime;
        }
        set
        {
            lifeTime = value;
        }
    }

	private TimeSpan creationTime;

	public VisualLog( string message)
	{
		this.message = CreationTime()  + message;
		this.color = Color.green;
		lifeTime = VISUALLOG_LIFETIME;
	}

	public VisualLog( string message, Color color)
	{
		this.message = CreationTime() + message;
		this.color = color;
		lifeTime = VISUALLOG_LIFETIME;
	}

	private string CreationTime()
	{
		creationTime = System.DateTime.Now.TimeOfDay;
		return "[" + creationTime.Hours + ":" + creationTime.Minutes + ":" + creationTime.Seconds + "] ";
	}
}