using System;
using System.Collections;
using CHARACTERS;
using DIALOGUE;
using UnityEngine;

public class GraphicLayerTesting : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RunningLayers());
    }

    private IEnumerator Running()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
        GraphicLayer layer = panel.GetLayer(0, true);
        
        yield return new WaitForSeconds(1);

        Texture blendTex = Resources.Load<Texture>("Graphics/Transition Effects/hurricane");
        layer.SetTexture("Graphics/BG Images/2", blendingTexture: blendTex);
        
        yield return new WaitForSeconds(1);
        
        layer.SetVideo("Graphics/BG Videos/Fantasy Landscape", blendingTexture: blendTex);
        
        yield return new WaitForSeconds(3);

        layer.currentGraphic.FadeOut();
        
        yield return new WaitForSeconds(1);

        Debug.Log(layer.currentGraphic);
    }

    private IEnumerator RunningLayers()
    {
        GraphicPanel panel = GraphicPanelManager.instance.GetPanel("Background");
        GraphicLayer layer0 = panel.GetLayer(0, true);
        GraphicLayer layer1 = panel.GetLayer(1, true);
        
        layer0.SetVideo("Graphics/BG Videos/Nebula");
        layer1.SetTexture("Graphics/BG Images/Spaceshipinterior");
        
        yield return new WaitForSeconds(2);
        
        GraphicPanel cinematic = GraphicPanelManager.instance.GetPanel("Cinematic");
        GraphicLayer cinLayer = cinematic.GetLayer(0, true);

        Character Stella = CharacterManager.instance.CreateCharacter("Stella", true);
        
        yield return Stella.Say("Let's take a look at a picture on the cinematic layer.");
        
        cinLayer.SetTexture("Graphics/Gallery/pup");

        yield return DialogueSystem.instance.Say("Narrator", "We truly don't deserve dogs.");
        
        cinLayer.Clear();
        
        yield return new WaitForSeconds(1);
        
        panel.Clear();
    }
}
