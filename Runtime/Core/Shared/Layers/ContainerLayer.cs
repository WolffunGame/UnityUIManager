﻿using System;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared.Layers
{
    public abstract class ContainerLayer : MonoBehaviour, IContainerLayer
    {
        public int Layer { get; protected set; }
        [SerializeField] private string layerName;

        public string LayerName
        {
            get
            {
                if (string.IsNullOrEmpty(layerName))
                {
                    layerName = gameObject.name;
                }

                return layerName;
            }
            protected set => layerName = value;
        }

        public abstract int VisibleElementInLayer { get; }
        public ContainerLayerType LayerType { get; set; }
        
        [SerializeField] private Canvas canvas;

        public Canvas Canvas
        {
            get
            {
                if(canvas == null)
                {
                    canvas = GetComponent<Canvas>();
                }

                return canvas;
            }
        }

        public abstract Window Current { get; }

        private IContainerLayerManager _containerLayerManager;

        public IContainerLayerManager ContainerLayerManager
        {
            get { return _containerLayerManager ??= FindObjectOfType<ContainerLayerManager>(); }
            set { _containerLayerManager = value; }
        }

        public void CreateLayer(string newLayerName, int layer, ContainerLayerType layerType)
        {
            LayerName = newLayerName;
            Layer = layer;
            LayerType = layerType;
            OnCreate();
            ContainerLayerManager.Add(this);
        }

        public abstract void OnBackButtonPressed();

        protected abstract void OnCreate();
        
        private void OnValidate()
        {
            if(canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }
        }
    }
}