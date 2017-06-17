using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{
    public class AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private float _camAngle = 0;
        private TransformComponent _baseTransform;
        private TransformComponent _unterarmTransform;
        private TransformComponent _verbagTransform;
        private TransformComponent _oberesGewTransform;
        private TransformComponent _RadR01Transform;
        private TransformComponent _RadR02Transform;
        private TransformComponent _RadR03Transform;
        private TransformComponent _RadR04Transform;
        private TransformComponent _RadR05Transform;
        private TransformComponent _RadR06Transform;
        //private TransformComponent _oberarmTransform;
        private ScenePicker _scenePicker;
        private PickResult _currentPick;
        private float3 _oldColor;
        SceneContainer CreateScene()
        {
            // Initialize transform components that need to be changed inside "RenderAFrame"
            _baseTransform = new TransformComponent
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };

            // Setup the scene graph
            return new SceneContainer
            {
                Children = new List<SceneNodeContainer>
                {
                    new SceneNodeContainer
                    {
                        Components = new List<SceneComponentContainer>
                        {
                            // TRANSFROM COMPONENT
                            _baseTransform,

                            // MATERIAL COMPONENT
                            new MaterialComponent
                            {
                                Diffuse = new MatChannelContainer { Color = new float3(0.7f, 0.7f, 0.7f) },
                                Specular = new SpecularChannelContainer { Color = new float3(1, 1, 1), Shininess = 5 }
                            },

                            // MESH COMPONENT
                            // SimpleAssetsPickinges.CreateCuboid(new float3(10, 10, 10))
                            SimpleMeshes.CreateCuboid(new float3(10, 10, 10))
                        }
                    },
                }
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.8f, 0.9f, 0.7f, 1);

            _scene = AssetStorage.Get<SceneContainer>("Fahrzeug3.fus");


            //Scene Picker
            _scenePicker = new ScenePicker(_scene);

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            //_baseTransform.Rotation = new float3(0, M.MinAngle(TimeSinceStart), 0);
            _oberesGewTransform = _scene.Children.FindNodes(node => node.Name == "pfeiler_gewinde_arm_nicht_drehbar")?.FirstOrDefault()?.GetTransform();
            _RadR01Transform = _scene.Children.FindNodes(node => node.Name == "Rad_R_01")?.FirstOrDefault()?.GetTransform();
            _RadR02Transform = _scene.Children.FindNodes(node => node.Name == "Rad_L_01")?.FirstOrDefault()?.GetTransform();
            _RadR03Transform = _scene.Children.FindNodes(node => node.Name == "Rad_02_R")?.FirstOrDefault()?.GetTransform();
            _RadR04Transform = _scene.Children.FindNodes(node => node.Name == "Rad_02_L")?.FirstOrDefault()?.GetTransform();
            _RadR05Transform = _scene.Children.FindNodes(node => node.Name == "Rad_03_L")?.FirstOrDefault()?.GetTransform();
            _RadR06Transform = _scene.Children.FindNodes(node => node.Name == "Rad_03_R")?.FirstOrDefault()?.GetTransform();
            _unterarmTransform = _scene.Children.FindNodes(node => node.Name == "unterer_arm")?.FirstOrDefault()?.GetTransform(); //Oberer_arm
            //_oberarmTransform = _scene.Children.FindNodes(node => node.Name == "Oberer_arm")?.FirstOrDefault()?.GetTransform();
           _verbagTransform = _scene.Children.FindNodes(node => node.Name == "verbindung_arm_greifer")?.FirstOrDefault()?.GetTransform();


            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);


            if (Mouse.RightButton == true)
            {
                _camAngle += Mouse.Velocity.x * 0.00001f * DeltaTime / 20 * 10000;
            }
            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, -2, 10) * float4x4.CreateRotationY(_camAngle);


            //ToDO If Bedingung eingefügt

            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);
                _scenePicker.View = RC.View;
                _scenePicker.Projection = RC.Projection;
                List<PickResult> pickResults = _scenePicker.Pick(pickPosClip).ToList();
                if (pickResults.Count > 0)
                {
                    Diagnostics.Log(pickResults[0].Node.Name);

                    PickResult newPick = null;
                    if (pickResults.Count > 0)
                    {
                        pickResults.Sort((a, b) => Sign(a.ClipPos.z - b.ClipPos.z));
                        newPick = pickResults[0];
                    }
                    if (newPick?.Node != _currentPick?.Node)
                    {
                        if (_currentPick != null)
                        {
                            _currentPick.Node.GetMaterial().Diffuse.Color = _oldColor;
                        }
                        if (newPick != null)
                        {
                            var mat = newPick.Node.GetMaterial();
                            _oldColor = mat.Diffuse.Color;
                            mat.Diffuse.Color = new float3(1, 0.4f, 0.4f);

                        }
                        _currentPick = newPick;
                    }
                }
            }


            if (_currentPick != null)
            {
                switch (_currentPick.Node.Name)
                {
                    case "Rad_R_01":
                        float RadR01 = _RadR01Transform.Rotation.x;
                        RadR01 += Keyboard.ADAxis * 2.0f * (DeltaTime);
                        _RadR01Transform.Rotation = new float3(RadR01, 0, 0);
                        break;
                    case "Rad_L_01":
                        float RadR02 = _RadR02Transform.Rotation.x;
                        RadR02 += Keyboard.ADAxis * 2.0f * (DeltaTime);
                        _RadR02Transform.Rotation = new float3(RadR02, 0, 0);
                        break;
                    case "Rad_02_R":
                        float RadR03 = _RadR03Transform.Rotation.x;
                        RadR03 += Keyboard.ADAxis * 2.0f * (DeltaTime);
                        _RadR03Transform.Rotation = new float3(RadR03, 0, 0);
                        break;
                    case "Rad_02_L":
                        float RadR04 = _RadR04Transform.Rotation.x;
                        RadR04 += Keyboard.ADAxis * 2.0f * (DeltaTime);
                        _RadR04Transform.Rotation = new float3(RadR04, 0, 0);
                        break;
                    case "Rad_03_L":
                        float RadR05 = _RadR05Transform.Rotation.x;
                        RadR05 += Keyboard.ADAxis * 2.0f * (DeltaTime);
                        _RadR05Transform.Rotation = new float3(RadR05, 0, 0);
                        break;
                    case "Rad_03_R":
                        float RadR06 = _RadR06Transform.Rotation.x;
                        RadR06 += Keyboard.ADAxis * 2.0f * (DeltaTime);
                        _RadR06Transform.Rotation = new float3(RadR06, 0, 0);
                        break;
                    


                    case "pfeiler_gewinde_arm_nicht_drehbar":
                        float ogd = _oberesGewTransform.Rotation.y;
                        ogd += Keyboard.LeftRightAxis * 0.005f;
                        _oberesGewTransform.Rotation =new float3(0, ogd, 0);
                        break;
                    case "unterer_arm":
                        float unter = _unterarmTransform.Rotation.x;
                        if (Keyboard.GetKey(KeyCodes.Up) == true)
                        {
                            if (unter <= 0.35f)
                            {
                               
                                unter += DeltaTime * 0.1f;
                                _unterarmTransform.Rotation = new float3(unter, 0, 0);
                            }
                        }
                        if (Keyboard.GetKey(KeyCodes.Down) == true)
                        {
                            if (unter >= 0.0f)
                            {

                                unter -= DeltaTime * 0.1f;
                                _unterarmTransform.Rotation = new float3(unter, 0, 0);
                            }
                        }
                        break;
                    /*case "Oberer_arm":
                        float obArmx = _oberarmTransform.Translation.z;
                        obArmx += Keyboard.UpDownAxis * 0.1f;
                        /*float obArmy = _oberarmTransform.Translation.z;
                        obArmy += Keyboard.UpDownAxis * 0.1f;*/
                        /*_oberarmTransform.Translation = new float3(0, 0, obArmx);
                        break;*/
                    case "verbindung_arm_greifer":
                        float ver = _verbagTransform.Translation.y;
                        if (Keyboard.GetKey(KeyCodes.Up) == true)
                        {
                            if (ver <= -0.5f)
                            {
                                ver += DeltaTime * 2f;
                                _verbagTransform.Translation = new float3(0, ver, 0);
                            }
                        }
                        if (Keyboard.GetKey(KeyCodes.Down) == true)
                        {
                            if (ver >= -10.0f)
                            {

                                ver -= DeltaTime * 2f;
                                _verbagTransform.Translation = new float3(0, ver, 0);
                            }
                        }
                        break; 
                }
            }
            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}
