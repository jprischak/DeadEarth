﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class CameraBloodEffect : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private Shader     _shader             = null;
    [SerializeField] private Texture2D  _bloodTexture       = null;
    [SerializeField] private Texture2D  _bloodNormalMap     = null;
    [SerializeField] private float      _bloodAmount        = 0.0f;
    [SerializeField] private float      _minBloodAmount     = 0.0f;
    [SerializeField] private float      _distortion         = 1.0f;
    [SerializeField] private bool       _autoFade           = true;
    [SerializeField] private float      _fadeSpeed          = 0.05f;



    // Private Variables
    private Material _material = null;



    // Properties
    public float bloodAmount    { get { return _bloodAmount; }      set { _bloodAmount = value; }  }
    public float minBloodAmount { get { return _minBloodAmount; }   set { _minBloodAmount = value; }  }
    public float fadeSpeed      { get { return _fadeSpeed; }        set { _fadeSpeed = value; } }
    public bool  autoFade       { get { return _autoFade; }         set { _autoFade = value; } }









    private void Update()
    {
        // If auto fade enabled then decrement bood amount but keep it above the min
        if(_autoFade)
        {
            _bloodAmount -= _fadeSpeed * Time.deltaTime;
            _bloodAmount = Mathf.Max(_bloodAmount, _minBloodAmount);
        }
    }




    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // If we don't have a shader then return
        if (_shader == null)    return;

        // If we don't have a material create one
        if(_material == null)   
            _material = new Material(_shader);
        
        // If we still don't have a material then return
        if (_material == null)  return;



        // Send data into shader
        if(_bloodTexture != null)
            _material.SetTexture("_BloodTex", _bloodTexture);
        if(_bloodNormalMap != null)
            _material.SetTexture("_BloodBump", _bloodNormalMap);

        _material.SetFloat("_Distortion", _distortion);
        _material.SetFloat("_BloodAmount", _bloodAmount);




        // Perform Image effect
        Graphics.Blit(source, destination, _material);
    }
}
