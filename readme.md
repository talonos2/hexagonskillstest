What I did:
 - All code you see was written by me, though some was developed by referencing the sources below:
 - The shadergraph was written by me.
 - The particle systems were written by me.

What I referenced to do what I did:
 - https://docs.unity3d.com/Manual/Example-CreatingaBillboardPlane.html because I've never built a mesh from scratch before.
 - https://forum.unity.com/threads/the-current-render-pipeline-is-not-compatible-with-this-master-node.660742/ when Shadergraph failed to work as expected.
 - https://i.ibb.co/N96B5YH/image.png to cure "Shadow Acne".
 - I looked at this guy's shadergraph and found out I needed to be setting my normals space to "tangent" in my master node: https://drive.google.com/file/d/1N8wu5wL6K96qINbjfL9KgH0I7oo2bjE5/view
 - I was trawling around for cool shader ideas and ran into Ben Cloward's directional moss shader, so I took the idea and modified it to work with my shader. It ended up not working due to a difference between the way Unreal and Unity handles clamping NaN values, so I stopped working on it because it was a time sink.

What I did not do:
 - I installed the Universal Rendering Pipeline package, built by Unity.
 - I installed the Shadergraph package, built by Unity.
 - I installed TextMeshPro.
 - The open hand icon. (https://www.svgrepo.com/svg/147219/pointing-right , It's CC0)
 - This guy put up some permissively licensed rock textures: https://drive.google.com/drive/folders/1w03IEM-VrteriQcs9Q7gI0mnkE2ABP5E
 - Moss textures from https://drive.google.com/drive/folders/1AckSrB5Ce_PbyzEewlATfmLWJTaXdQ1G
 - Music is called the "This is it! Concerto", bu Joshua Cloward. It's used in my side game, Final winter.
 - The Sound Manager, Music Manager, and Music Loop scripts are 100% my code, but were copied from previous projects I've done.

If had the opportunity to do this again:
 - I wouldn't have wasted time on normal maps, ambient occlusion maps, etc. The stone was supposed to turn out looking like this: https://3dtextures.me/2020/08/25/stone-wall-016/ but apparently you can't include tesselation into a shader made by shadergraph, and the tesselation shader's what makes that texture look so cool. I might as well have just used the base albedo+ambient.
 - I do a *Lot* of fading in/out of UI components. I think instead of handling it all in GameManager, I'd try out making a single component that does fading that can attach to each UI component; it'd make the code cleaner to read.