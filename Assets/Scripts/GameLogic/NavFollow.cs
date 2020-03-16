﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NavFollow : MonoBehaviour
{
    public Pathfinding pathfinder;
    public LayerMask raycastMask;
    public AreaManager areaManager;
    public SpriteMask timerMask;
    public SpriteMask noiseMask;

    private float m_CurrentNoise = 0.0f;
    private bool m_Stopped = true;
    private float m_BaseSpeed = 0.5f;
    private List<Vector3> m_CornerNodes;
    private int m_CornersIterator = 0;
    private float m_SpeedModifier = 1.0f;

    public void Go()
    {
        m_Stopped = false;
    }

    public void Setup(AreaManager areaManager, Pathfinding pathfinder)
    {
        this.areaManager = areaManager;
        this.pathfinder = pathfinder;

        if (this.areaManager)
        {
            this.areaManager.eventStartTimer += Go;
            this.areaManager.eventStartTimer += ResetNoise;
            this.areaManager.eventEndTimer += Stop;
        }

        Stop();
    }

    private void ResetNoise()
    {
        m_CurrentNoise = 0;
    }

    public void Stop()
    {
        m_Stopped = true;
    }

    public void SetSpeedModifier(float modifier, float seconds)
    {
        if (modifier <= 0)
        {
            modifier = float.Epsilon;
        }
        StartCoroutine(ModifySpeedCoroutine(modifier, seconds));
    }

    private IEnumerator ModifySpeedCoroutine(float modifier, float seconds)
    {
        m_SpeedModifier *= modifier;
        yield return new WaitForSeconds(seconds);
        m_SpeedModifier /= modifier;
    }

    private void Update()
    {
        if (timerMask && areaManager)
        {
            timerMask.alphaCutoff = areaManager.remainingTimeFraction;
        }

        if (noiseMask)
        {
            noiseMask.alphaCutoff = m_CurrentNoise;
        }

        if (m_Stopped)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, raycastMask))
            {
                m_CornerNodes = pathfinder.FindPath(transform.position, hit.point);
                m_CornersIterator = 0;
            }
        }

        if (m_CornerNodes != null && m_CornersIterator < m_CornerNodes.Count && m_CornerNodes.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_CornerNodes[m_CornersIterator], m_BaseSpeed * m_SpeedModifier * Time.deltaTime);
            if (Vector3.Distance(transform.position, m_CornerNodes[m_CornersIterator]) < Vector3.kEpsilon)
            {
                m_CornersIterator++;
            }

            m_CurrentNoise += 0.4f * Time.deltaTime;
            Mathf.Clamp01(m_CurrentNoise);
        }

        if (m_CurrentNoise >= 1)
        {
            Stop();
            return;
        }

        if (m_CurrentNoise > 0)
        {
            m_CurrentNoise -= 0.25f * Time.deltaTime;
            Mathf.Clamp01(m_CurrentNoise);
        }
    }
}
