using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Wave : MonoBehaviour
{
	public int resolution = 60;
	int current;

	float xScale;
	float yScale = 0.5f;

	float width = Screen.width;

	public Vector3 pos;

	float[] waveForm;
	float[] samples;
	public AudioSource source;
	public Material mat, rectMat;

	public Color waveformColour;

	float startLoop;
	float endLoop;

	public bool isLooping, isReversed;
	
	void Start()
	{
		resolution = source.clip.frequency / resolution;

		samples = new float[source.clip.samples * source.clip.channels];
		source.clip.GetData(samples, 0);

		waveForm = new float[(samples.Length / resolution)];

		for (int i = 0; i < waveForm.Length; i++)
		{
			waveForm[i] = 0;

			for (int j = 0; j < resolution; j++)
			{
				waveForm[i] += Mathf.Abs(samples[(i * resolution) + j]);
			}

			waveForm[i] /= resolution;
		}

		xScale = width / samples.Length;
		
		endLoop = width;

        source.loop = true;
        
	}

	void OnPostRender()
	{
		DrawWaveform();

		DrawCursor();
		
		if(isLooping)
			DrawLoopHandles();

        if (isReversed)
            source.pitch = -1;
        else
            source.pitch = 0;
    }

	private void DrawLoopHandles()
	{
		Vector3 start = new Vector3(startLoop / width + pos.x, 0, 0);
		Vector3 end = new Vector3(endLoop / width + pos.x, 0, 0);

		DrawLine2D(start, start + Vector3.up * 10, Color.red);
		DrawLine2D(end, end + Vector3.up * 10, Color.red);
		
		DrawLine2D(new Vector3(startLoop / width + pos.x, 0.5f, 0),
			new Vector3(endLoop / width + pos.x, 0.5f, 0),
			Color.red);

		DrawRect(Vector3.zero, new Vector3(start.x, 10, 0));
		DrawRect(new Vector3(end.x, 0, 0), new Vector3(width, 10, 0));
	}

	private void DrawRect(Vector3 bottomLeft, Vector3 topRight)
	{
		GL.PushMatrix();
		rectMat.color = new Color(0.5f,0.5f,0.5f,0.5f);
		rectMat.SetPass(0);
		GL.LoadOrtho();
		
		GL.Begin(GL.QUADS);
		GL.Vertex(bottomLeft);
		GL.Vertex(new Vector3(bottomLeft.x, topRight.y, 0));
		GL.Vertex(topRight);
		GL.Vertex(new Vector3(topRight.x, bottomLeft.x, 0));

		GL.End();
		GL.PopMatrix();
	}

	private void DrawCursor()
	{
		current = source.timeSamples / resolution;
		current *= 2;
		
		Vector3 c = new Vector3(current * xScale + pos.x, 0, 0);

		DrawLine2D(c, c + Vector3.up * 10, Color.green);
	}

	private void DrawWaveform()
	{
		GL.PushMatrix();
		mat.color = waveformColour;
		mat.SetPass(0);
		GL.LoadOrtho();

        if (isReversed)
        {
            for (int i = waveForm.Length -1; i > 0; i--)
                DrawSample(i);
        }
        else
        {
            for (int i = 0; i < waveForm.Length - 1; i++)
                DrawSample(i);
        }
	
		GL.PopMatrix();
	}

    void DrawSample(int i)
    {
        GL.Begin(GL.LINES);
        GL.Color(waveformColour);

        Vector3 start = new Vector3(i * xScale + pos.x, waveForm[i] * yScale + pos.y, 0);
        Vector3 end = new Vector3(i * xScale + pos.x, -waveForm[i] * yScale + pos.y, 0);

        GL.Vertex(start);
        GL.Vertex(end);

        GL.End();

        if (i % resolution == 0)
        {
            Vector3 secondPos = new Vector3(i * xScale + pos.x, 0, 0);
            DrawLine2D(secondPos, secondPos + Vector3.up * 2, Color.white);
        }
    }

	void Update()
	{
		if (isLooping)
		{
			if (Input.GetMouseButtonDown(0))
			{
				if (Input.mousePosition.x > endLoop)
					startLoop = endLoop;
				else
					startLoop = Input.mousePosition.x;
			}

			if (Input.GetMouseButtonDown(1))
			{
				if (Input.mousePosition.x < startLoop)
					endLoop = startLoop;
				else
					endLoop = Input.mousePosition.x;
			}

            if (Input.GetKeyDown(KeyCode.R))
                isReversed = !isReversed;

			int startSample = (int)(((startLoop * resolution) / xScale) / width / 2);
            int endSample = (int)(((endLoop * resolution) / xScale) / width / 2);

            //Update loop position

            if (isReversed)
            {
                if (current >= (endLoop / width) / xScale)
                    source.timeSamples = endSample;

                if (current <= (startLoop / width) / xScale)
                    source.timeSamples = endSample;
            }
            else
            {
                if (current >= (endLoop / width) / xScale)
                    source.timeSamples = startSample;

                if (current <= (startLoop / width) / xScale)
                    source.timeSamples = startSample;
            }
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (source.isPlaying)
				source.Pause();
			else
				source.UnPause();
		}
		
	}

	void DrawLine2D(Vector3 start, Vector3 end, Color color)
	{
		GL.PushMatrix();
		mat.color = color;
		mat.SetPass(0);
		
		GL.LoadOrtho();

		GL.Begin(GL.LINES);
		GL.Color(color);

		GL.Vertex(start);
		GL.Vertex(end);

		GL.End();
		GL.PopMatrix();
	}
}