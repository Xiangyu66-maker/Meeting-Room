$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$SceneDir = Join-Path $ProjectRoot 'Assets\Scenes'
$DocDir = Join-Path $ProjectRoot 'Assets\Documentation'
$ShotDir = Join-Path $DocDir 'Screenshots'
$MaterialDir = Join-Path $ProjectRoot 'Assets\Materials\Blockout'
New-Item -ItemType Directory -Force -Path $SceneDir, $DocDir, $ShotDir, $MaterialDir | Out-Null

$Culture = [System.Globalization.CultureInfo]::InvariantCulture
function F([double]$Value) { $Value.ToString('0.###', $Culture) }
function Vec($Value) { "{x: $(F $Value[0]), y: $(F $Value[1]), z: $(F $Value[2])}" }
function JsonEscape([string]$Value) {
    $Value.Replace('\', '\\').Replace('"', '\"').Replace("`r", '\r').Replace("`n", '\n')
}
function B([string]$Hex) {
    New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml($Hex))
}
function Get-BlockoutMaterialGuid([string]$Id) {
    $Md5 = [System.Security.Cryptography.MD5]::Create()
    try {
        $Bytes = [System.Text.Encoding]::UTF8.GetBytes("SURF0644.Blockout.Material.$Id")
        (($Md5.ComputeHash($Bytes) | ForEach-Object { $_.ToString('x2') }) -join '')
    }
    finally {
        $Md5.Dispose()
    }
}
function Convert-HexToUnityColor([string]$Hex) {
    $Clean = $Hex.TrimStart('#')
    @(
        ([Convert]::ToInt32($Clean.Substring(0, 2), 16) / 255.0),
        ([Convert]::ToInt32($Clean.Substring(2, 2), 16) / 255.0),
        ([Convert]::ToInt32($Clean.Substring(4, 2), 16) / 255.0)
    )
}
function Write-BlockoutMaterial($Object) {
    $Guid = Get-BlockoutMaterialGuid $Object.Id
    $Name = "Blockout_$($Object.Id)"
    $Path = Join-Path $MaterialDir "$Name.mat"
    $MetaPath = "$Path.meta"
    $Color = Convert-HexToUnityColor $Object.Color
    $R = F $Color[0]
    $G = F $Color[1]
    $B = F $Color[2]
    $Material = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 6
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: $Name
  m_Shader: {fileID: 45, guid: 0000000000000000f000000000000000, type: 0}
  m_ShaderKeywords: 
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {}
  disabledShaderPasses: []
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _BumpMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _DetailAlbedoMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _DetailMask:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _DetailNormalMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _EmissionMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _MainTex:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _OcclusionMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _ParallaxMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _SpecGlossMap:
        m_Texture: {fileID: 0}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    m_Floats:
    - _BumpScale: 1
    - _Cutoff: 0.5
    - _DetailNormalMapScale: 1
    - _DstBlend: 0
    - _GlossMapScale: 1
    - _Glossiness: 0.25
    - _GlossyReflections: 1
    - _Mode: 0
    - _OcclusionStrength: 1
    - _Parallax: 0.02
    - _SmoothnessTextureChannel: 0
    - _SpecularHighlights: 1
    - _SrcBlend: 1
    - _UVSec: 0
    - _ZWrite: 1
    m_Colors:
    - _Color: {r: $R, g: $G, b: $B, a: 1}
    - _EmissionColor: {r: 0, g: 0, b: 0, a: 1}
    - _SpecColor: {r: 0.2, g: 0.2, b: 0.2, a: 1}
"@
    $Meta = @"
fileFormatVersion: 2
guid: $Guid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 2100000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    [System.IO.File]::WriteAllText($Path, $Material, [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText($MetaPath, $Meta, [System.Text.UTF8Encoding]::new($false))
}

$Objects = @(
    [pscustomobject]@{ Id='room_floor'; Cn='房间地面'; Cat='地图结构'; Area='会议室全区，12m x 10m'; Role='定义可行走地面与整体地图范围，扩大后方便玩家移动'; Handoff='S2/S3/S4'; P=@(0,-0.05,0); S=@(12,0.1,10); Color='#8A8C85'; Trigger=0 },
    [pscustomobject]@{ Id='wall_front'; Cn='前墙'; Cat='地图结构'; Area='Z=5，显示屏所在墙'; Role='封闭房间前侧边界，承载 screen_01'; Handoff='S2/S4'; P=@(0,1.5,5); S=@(12,3,0.1); Color='#B8B9B3'; Trigger=0 },
    [pscustomobject]@{ Id='wall_back'; Cn='后墙'; Cat='地图结构'; Area='Z=-5，玩家出生点后方'; Role='封闭房间后侧边界，start_point 位于墙内侧'; Handoff='S2/S4'; P=@(0,1.5,-5); S=@(12,3,0.1); Color='#B8B9B3'; Trigger=0 },
    [pscustomobject]@{ Id='wall_left'; Cn='左墙'; Cat='地图结构'; Area='X=-6，白板侧'; Role='封闭房间左侧边界，承载 whiteboard_01'; Handoff='S2/S4'; P=@(-6,1.5,0); S=@(0.1,3,10); Color='#B8B9B3'; Trigger=0 },
    [pscustomobject]@{ Id='wall_right'; Cn='右墙'; Cat='地图结构'; Area='X=6，储物柜侧'; Role='封闭房间右侧边界，靠近 cabinet_01'; Handoff='S2/S4'; P=@(6,1.5,0); S=@(0.1,3,10); Color='#B8B9B3'; Trigger=0 },
    [pscustomobject]@{ Id='ceiling_01'; Cn='天花板'; Cat='地图结构'; Area='房间顶部'; Role='封闭会议室上方空间，形成完整室内房间'; Handoff='S2/S4'; P=@(0,3.05,0); S=@(12,0.1,10); Color='#C9CAC4'; Trigger=0 },
    [pscustomobject]@{ Id='meeting_table_01'; Cn='会议桌'; Cat='家具'; Area='会议室中央'; Role='承载 laptop_01、wrong_cable_01、document_01 和 table_target_area'; Handoff='S2/S3/S4'; P=@(0,0.4,0); S=@(3,0.2,1.5); Color='#6B4221'; Trigger=0 },
    [pscustomobject]@{ Id='chair_01'; Cn='椅子'; Cat='家具'; Area='会议桌后排左侧'; Role='后续 T7 调整椅子的候选对象'; Handoff='S2/S3/S4'; P=@(-1.2,0.25,-1.2); S=@(0.5,0.5,0.5); Color='#0F5C57'; Trigger=0 },
    [pscustomobject]@{ Id='chair_02'; Cn='椅子'; Cat='家具'; Area='会议桌后排中间'; Role='后续 T7 调整椅子的候选对象'; Handoff='S2/S3/S4'; P=@(0,0.25,-1.2); S=@(0.5,0.5,0.5); Color='#0F5C57'; Trigger=0 },
    [pscustomobject]@{ Id='chair_03'; Cn='椅子'; Cat='家具'; Area='会议桌后排右侧'; Role='后续 T7 调整椅子的候选对象'; Handoff='S2/S3/S4'; P=@(1.2,0.25,-1.2); S=@(0.5,0.5,0.5); Color='#0F5C57'; Trigger=0 },
    [pscustomobject]@{ Id='chair_04'; Cn='椅子'; Cat='家具'; Area='会议桌前排左侧'; Role='后续 T7 调整椅子的候选对象'; Handoff='S2/S3/S4'; P=@(-1.2,0.25,1.2); S=@(0.5,0.5,0.5); Color='#0F5C57'; Trigger=0 },
    [pscustomobject]@{ Id='chair_05'; Cn='椅子'; Cat='家具'; Area='会议桌前排中间'; Role='后续 T7 调整椅子的候选对象'; Handoff='S2/S3/S4'; P=@(0,0.25,1.2); S=@(0.5,0.5,0.5); Color='#0F5C57'; Trigger=0 },
    [pscustomobject]@{ Id='chair_06'; Cn='椅子'; Cat='家具'; Area='会议桌前排右侧'; Role='后续 T7 调整椅子的候选对象'; Handoff='S2/S3/S4'; P=@(1.2,0.25,1.2); S=@(0.5,0.5,0.5); Color='#0F5C57'; Trigger=0 },
    [pscustomobject]@{ Id='cabinet_01'; Cn='储物柜'; Cat='家具'; Area='右侧墙边，X=5.1'; Role='放置 hdmi_cable_01 和 remote_01 的搜索区域'; Handoff='S2/S3/S4'; P=@(5.1,0.75,1.2); S=@(1,1.5,0.5); Color='#574C40'; Trigger=0 },
    [pscustomobject]@{ Id='whiteboard_01'; Cn='白板'; Cat='家具'; Area='左侧墙面，X=-5.93'; Role='会议室视觉参照物，可作为后续提示区域'; Handoff='S2/S3/S4'; P=@(-5.93,1.45,1.0); S=@(0.08,1.1,1.8); Color='#EBEFE6'; Trigger=0 },
    [pscustomobject]@{ Id='screen_01'; Cn='显示屏'; Cat='设备'; Area='房间前方墙面，Z=4.93'; Role='T4 连接显示内容，T5 被遥控器打开'; Handoff='S2/S3/S4'; P=@(0,1.55,4.93); S=@(2.5,1.2,0.08); Color='#143D94'; Trigger=0 },
    [pscustomobject]@{ Id='laptop_01'; Cn='笔记本电脑'; Cat='任务物体'; Area='会议桌左侧'; Role='T1 找到并拿起电脑'; Handoff='S2/S3/S4'; P=@(-0.8,0.6,0); S=@(0.6,0.05,0.4); Color='#15171A'; Trigger=0 },
    [pscustomobject]@{ Id='hdmi_cable_01'; Cn='HDMI 线'; Cat='任务物体'; Area='储物柜上方'; Role='T2 正确目标线缆，拿到后连接 laptop_01 与 screen_01'; Handoff='S2/S3/S4'; P=@(5.1,1.55,1.0); S=@(0.7,0.04,0.04); Color='#1266E6'; Trigger=0 },
    [pscustomobject]@{ Id='wrong_cable_01'; Cn='错误线缆'; Cat='干扰物'; Area='会议桌右侧'; Role='干扰项；S2 可用作拿错线缆的失败条件'; Handoff='S2/S3/S4'; P=@(0.8,0.6,0.3); S=@(0.7,0.04,0.04); Color='#DB261A'; Trigger=0 },
    [pscustomobject]@{ Id='remote_01'; Cn='遥控器'; Cat='任务物体'; Area='储物柜上方'; Role='T5 打开 screen_01'; Handoff='S2/S3/S4'; P=@(4.9,1.6,1.3); S=@(0.25,0.04,0.1); Color='#080809'; Trigger=0 },
    [pscustomobject]@{ Id='document_01'; Cn='会议资料'; Cat='任务物体'; Area='会议桌边缘'; Role='T6 摆放到 table_target_area'; Handoff='S2/S3/S4'; P=@(0.2,0.62,-0.3); S=@(0.4,0.02,0.3); Color='#F5EDC7'; Trigger=0 },
    [pscustomobject]@{ Id='table_target_area'; Cn='桌面目标区域'; Cat='区域点'; Area='会议桌中心'; Role='document_01 的放置目标区域'; Handoff='S2/S3/S4'; P=@(0,0.635,0); S=@(0.8,0.03,0.5); Color='#26CC52'; Trigger=1 },
    [pscustomobject]@{ Id='start_point'; Cn='玩家起点'; Cat='区域点'; Area='会议室内部后方，Z=-4.2'; Role='玩家出生点，已放在封闭会议室内部并留出移动空间'; Handoff='S2/S4'; P=@(0,0.035,-4.2); S=@(0.65,0.07,0.65); Color='#2ECC47'; Trigger=1 },
    [pscustomobject]@{ Id='presentation_point'; Cn='演讲站位点'; Cat='区域点'; Area='显示屏前方，Z=3.3'; Role='最终站位；任务完成点'; Handoff='S2/S3/S4'; P=@(0,0.035,3.3); S=@(0.65,0.07,0.65); Color='#FAAD29'; Trigger=1 }
)

foreach ($Object in $Objects) {
    Write-BlockoutMaterial $Object
}

$ScriptGuid = (Select-String -LiteralPath (Join-Path $ProjectRoot 'Assets\Scripts\ObjectIdentity.cs.meta') -Pattern '^guid:').Line.Split(':')[1].Trim()

$Header = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 10
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.6, g: 0.61, b: 0.64, a: 1}
  m_AmbientEquatorColor: {r: 0.45, g: 0.45, b: 0.45, a: 1}
  m_AmbientGroundColor: {r: 0.35, g: 0.35, b: 0.35, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 0
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 13
  m_BakeOnSceneLoad: 0
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 1
    m_EnableRealtimeLightmaps: 0
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_NavMeshData: {fileID: 0}
"@

function New-ObjectYaml($Object, [int]$Index) {
    $Base = 100000 + ($Index * 10)
    $Go = $Base
    $Tr = $Base + 1
    $Mf = $Base + 2
    $Bc = $Base + 3
    $Mr = $Base + 4
    $Mb = $Base + 5
    $MatGuid = Get-BlockoutMaterialGuid $Object.Id
@"
--- !u!1 &$Go
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: $Tr}
  - component: {fileID: $Mf}
  - component: {fileID: $Bc}
  - component: {fileID: $Mr}
  - component: {fileID: $Mb}
  m_Layer: 0
  m_Name: $($Object.Id)
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &$Tr
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: $(Vec $Object.P)
  m_LocalScale: $(Vec $Object.S)
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: 0}
  m_RootOrder: $Index
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!33 &$Mf
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  m_Mesh: {fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}
--- !u!65 &$Bc
BoxCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  m_Material: {fileID: 0}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: $($Object.Trigger)
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Size: {x: 1, y: 1, z: 1}
  m_Center: {x: 0, y: 0, z: 0}
--- !u!23 &$Mr
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  m_Enabled: 1
  m_CastShadows: 1
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 2
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_ForceMeshLod: -1
  m_MeshLodSelectionBias: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {fileID: 2100000, guid: $MatGuid, type: 2}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {fileID: 0}
  m_ProbeAnchor: {fileID: 0}
  m_LightProbeVolumeOverride: {fileID: 0}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 1
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 3
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {fileID: 0}
  m_GlobalIlluminationMeshLod: 0
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_MaskInteraction: 0
  m_AdditionalVertexStreams: {fileID: 0}
--- !u!114 &$Mb
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: $ScriptGuid, type: 3}
  m_Name: 
  m_EditorClassIdentifier: 
  object_id: $($Object.Id)
"@
}

function New-PlayerPrefabYaml([int]$RootOrder) {
    $PlayerPrefabGuid = 'c6453f8e1f814744d8b94e5a6d1f9942'
@"
--- !u!1001 &900000
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 0}
    m_Modifications:
    - target: {fileID: 100006, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_Name
      value: Player
      objectReference: {fileID: 0}
    - target: {fileID: 400000, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalPosition.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400000, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalPosition.y
      value: 0.5
      objectReference: {fileID: 0}
    - target: {fileID: 400000, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalPosition.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_RootOrder
      value: $RootOrder
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalPosition.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalPosition.y
      value: 0.85
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalPosition.z
      value: -4.2
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalRotation.w
      value: 1
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalRotation.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalRotation.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalRotation.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalEulerAnglesHint.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalEulerAnglesHint.y
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 400002, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_LocalEulerAnglesHint.z
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 2000000, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_Enabled
      value: 1
      objectReference: {fileID: 0}
    - target: {fileID: 12410568, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_Enabled
      value: 1
      objectReference: {fileID: 0}
    - target: {fileID: 13600000, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_Height
      value: 1.4
      objectReference: {fileID: 0}
    - target: {fileID: 13600000, guid: $PlayerPrefabGuid, type: 3}
      propertyPath: m_Radius
      value: 0.35
      objectReference: {fileID: 0}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: $PlayerPrefabGuid, type: 3}
"@
}

function New-SceneRootsYaml([int]$PlayerPrefabInstanceId) {
    $Builder = New-Object System.Text.StringBuilder
    [void]$Builder.AppendLine('--- !u!1660057539 &9223372036854775807')
    [void]$Builder.AppendLine('SceneRoots:')
    [void]$Builder.AppendLine('  m_ObjectHideFlags: 0')
    [void]$Builder.AppendLine('  m_Roots:')
    for ($i = 0; $i -lt $Objects.Count; $i++) {
        $TransformId = 100000 + ($i * 10) + 1
        [void]$Builder.AppendLine("  - {fileID: $TransformId}")
    }
    [void]$Builder.AppendLine("  - {fileID: $PlayerPrefabInstanceId}")
    $Builder.ToString()
}

$ScenePath = Join-Path $SceneDir 'MeetingRoom_Blockout.unity'
$ConferenceScenePath = Join-Path $SceneDir 'ConferenceRoom.unity'
$Scene = New-Object System.Text.StringBuilder
[void]$Scene.AppendLine($Header.TrimEnd())
for ($i = 0; $i -lt $Objects.Count; $i++) {
    [void]$Scene.AppendLine((New-ObjectYaml $Objects[$i] $i).TrimEnd())
}
[void]$Scene.AppendLine((New-PlayerPrefabYaml $Objects.Count).TrimEnd())
[void]$Scene.AppendLine((New-SceneRootsYaml 900000).TrimEnd())
[System.IO.File]::WriteAllText($ScenePath, $Scene.ToString(), [System.Text.UTF8Encoding]::new($false))
[System.IO.File]::WriteAllText($ConferenceScenePath, $Scene.ToString(), [System.Text.UTF8Encoding]::new($false))

$SceneMetaPath = "$ScenePath.meta"
if (!(Test-Path -LiteralPath $SceneMetaPath)) {
    $Guid = [Guid]::NewGuid().ToString('N')
    $Meta = "fileFormatVersion: 2`nguid: $Guid`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n"
    [System.IO.File]::WriteAllText($SceneMetaPath, $Meta, [System.Text.UTF8Encoding]::new($false))
}

$ConferenceSceneMetaPath = "$ConferenceScenePath.meta"
if (!(Test-Path -LiteralPath $ConferenceSceneMetaPath)) {
    $Meta = "fileFormatVersion: 2`nguid: 9fc0d4010bbf28b4594072e72b8655ab`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n"
    [System.IO.File]::WriteAllText($ConferenceSceneMetaPath, $Meta, [System.Text.UTF8Encoding]::new($false))
}

$Doc = New-Object System.Text.StringBuilder
[void]$Doc.AppendLine('# MeetingRoom_Blockout_v3 Object ID Handoff')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('统一规则：所有关键物体的 Unity GameObject 名称等于 `object_id`，并且挂载 `ObjectIdentity` 组件，字段名为 `object_id`。学生 2、3、4 后续都只引用下表 ID。')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('## 场景截图')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('- 地图俯视截图：`Assets/Documentation/Screenshots/meeting_room_blockout_map.png`')
[void]$Doc.AppendLine('- 第一人称截图：`Assets/Documentation/Screenshots/meeting_room_blockout_first_person.png`')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('## 场景同步说明')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('`Assets/Scenes/ConferenceRoom.unity` 和 `Assets/Scenes/MeetingRoom_Blockout.unity` 已同步为同一套任务实验会议室，两个场景都使用下表这 24 个 `object_id`。会议室区域为 12m x 10m：`room_floor` 位于 `(0, -0.05, 0)`，四面墙位于 `X=±6`、`Z=±5`，`start_point` 位于会议室内部后方 `(0, 0.035, -4.2)`，`Player` prefab 出生位置为 `(0, 0.85, -4.2)` 并面向 `screen_01`。')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('## Object ID 表')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('| Object ID | 中文名称 | 类型 | 所在区域 / 坐标 | 任务作用 | 交接对象 |')
[void]$Doc.AppendLine('| --- | --- | --- | --- | --- | --- |')
foreach ($Object in $Objects) {
    $Pos = "($(F $Object.P[0]), $(F $Object.P[1]), $(F $Object.P[2]))"
    [void]$Doc.AppendLine("| ``$($Object.Id)`` | $($Object.Cn) | $($Object.Cat) | $($Object.Area) / $Pos | $($Object.Role) | $($Object.Handoff) |")
}
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('## 给学生 2：HTG / 任务图')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('建议正常任务路线：`start_point` -> `laptop_01` -> `cabinet_01` -> `hdmi_cable_01` -> `screen_01` -> `remote_01` -> `document_01` -> `table_target_area` -> `presentation_point`。')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('- 成功关键对象：`laptop_01`、`hdmi_cable_01`、`remote_01`、`document_01`、`table_target_area`、`presentation_point`。')
[void]$Doc.AppendLine('- 干扰对象：`wrong_cable_01`，位于会议桌右侧；可作为拿错线缆或错误选择的失败条件。')
[void]$Doc.AppendLine('- 椅子对象：`chair_01` 到 `chair_06`，后续可作为调整座位或清理动线任务。')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('## 给学生 3：高亮 / Act 层')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('优先支持高亮：`laptop_01`、`hdmi_cable_01`、`wrong_cable_01`、`remote_01`、`screen_01`、`document_01`、`table_target_area`、`presentation_point`、`cabinet_01`。')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('```json')
[void]$Doc.AppendLine('{')
[void]$Doc.AppendLine('  "type": "highlight_object",')
[void]$Doc.AppendLine('  "object_id": "hdmi_cable_01"')
[void]$Doc.AppendLine('}')
[void]$Doc.AppendLine('```')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('## 给学生 4：API / VLM 返回 JSON')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('API 返回时请使用稳定 `object_id`，不要返回自然语言描述。')
[void]$Doc.AppendLine('')
[void]$Doc.AppendLine('```json')
[void]$Doc.AppendLine('{')
[void]$Doc.AppendLine('  "target_object": "hdmi_cable_01",')
[void]$Doc.AppendLine('  "target_area": "table_target_area"')
[void]$Doc.AppendLine('}')
[void]$Doc.AppendLine('```')
[System.IO.File]::WriteAllText((Join-Path $DocDir 'Object_ID_List.md'), $Doc.ToString(), [System.Text.UTF8Encoding]::new($true))

$CsvRows = foreach ($Object in $Objects) {
    [pscustomobject]@{
        object_id = $Object.Id
        chinese_name = $Object.Cn
        category = $Object.Cat
        area = $Object.Area
        position = "($(F $Object.P[0]), $(F $Object.P[1]), $(F $Object.P[2]))"
        task_role = $Object.Role
        handoff = $Object.Handoff
    }
}
$CsvRows | Export-Csv -LiteralPath (Join-Path $DocDir 'Object_ID_List.csv') -NoTypeInformation -Encoding UTF8

$Json = New-Object System.Text.StringBuilder
[void]$Json.AppendLine('[')
for ($i = 0; $i -lt $Objects.Count; $i++) {
    $Object = $Objects[$i]
    [void]$Json.AppendLine('  {')
    [void]$Json.AppendLine("    `"object_id`": `"$(JsonEscape $Object.Id)`",")
    [void]$Json.AppendLine("    `"chinese_name`": `"$(JsonEscape $Object.Cn)`",")
    [void]$Json.AppendLine("    `"category`": `"$(JsonEscape $Object.Cat)`",")
    [void]$Json.AppendLine("    `"area`": `"$(JsonEscape $Object.Area)`",")
    [void]$Json.AppendLine("    `"position`": [$(F $Object.P[0]), $(F $Object.P[1]), $(F $Object.P[2])],")
    [void]$Json.AppendLine("    `"task_role`": `"$(JsonEscape $Object.Role)`",")
    [void]$Json.AppendLine("    `"handoff`": `"$(JsonEscape $Object.Handoff)`"")
    if ($i -eq $Objects.Count - 1) { [void]$Json.AppendLine('  }') } else { [void]$Json.AppendLine('  },') }
}
[void]$Json.AppendLine(']')
[System.IO.File]::WriteAllText((Join-Path $DocDir 'Object_ID_List.json'), $Json.ToString(), [System.Text.UTF8Encoding]::new($true))

function DrawLabel($Graphics, [string]$Text, [float]$X, [float]$Y, $Font, $Brush) {
    $Size = $Graphics.MeasureString($Text, $Font)
    $Graphics.DrawString($Text, $Font, $Brush, $X - $Size.Width / 2, $Y - $Size.Height / 2)
}

$Width = 1920
$Height = 1080
$Map = New-Object System.Drawing.Bitmap $Width, $Height
$G = [System.Drawing.Graphics]::FromImage($Map)
$G.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$G.Clear([System.Drawing.Color]::FromArgb(238,240,242))
$TitleFont = New-Object System.Drawing.Font('Arial', 28, [System.Drawing.FontStyle]::Bold)
$Font = New-Object System.Drawing.Font('Arial', 16, [System.Drawing.FontStyle]::Regular)
$SmallFont = New-Object System.Drawing.Font('Arial', 12, [System.Drawing.FontStyle]::Regular)
$Pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(35,35,35), 2)
$Scale = [Math]::Min(($Width - 180) / 12.0, ($Height - 260) / 10.0)
function MX([double]$X) { [float]($Width / 2 + $X * $Scale) }
function MY([double]$Z) { [float](($Height + 60) / 2 - $Z * $Scale) }
$G.FillRectangle((B '#8A8C85'), (MX -6), (MY 5), [float](12 * $Scale), [float](10 * $Scale))
$G.DrawRectangle($Pen, (MX -6), (MY 5), [float](12 * $Scale), [float](10 * $Scale))
$G.DrawString('MeetingRoom_Blockout_v3 - Expanded Closed 12m x 10m Map / Object IDs', $TitleFont, [System.Drawing.Brushes]::Black, 90, 38)
foreach ($Object in $Objects) {
    if ($Object.Id -like 'wall_*' -or $Object.Id -in @('room_floor','ceiling_01')) { continue }
    $X = MX ($Object.P[0] - $Object.S[0] / 2)
    $Y = MY ($Object.P[2] + $Object.S[2] / 2)
    $W = [float]($Object.S[0] * $Scale)
    $H = [float]($Object.S[2] * $Scale)
    $G.FillRectangle((B $Object.Color), $X, $Y, $W, $H)
    $G.DrawRectangle($Pen, $X, $Y, $W, $H)
    if ($Object.Cat -ne '家具' -or $Object.Id -in @('meeting_table_01','cabinet_01','whiteboard_01')) {
        DrawLabel $G $Object.Id (MX $Object.P[0]) (MY $Object.P[2]) $SmallFont ([System.Drawing.Brushes]::Black)
    }
}
DrawLabel $G 'front / screen wall' (MX 0) (MY 5.25) $Font ([System.Drawing.Brushes]::Black)
DrawLabel $G 'closed room - player spawn inside' (MX 0) (MY -4.7) $Font ([System.Drawing.Brushes]::Black)
$Map.Save((Join-Path $ShotDir 'meeting_room_blockout_map.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$G.Dispose()
$Map.Dispose()

$View = New-Object System.Drawing.Bitmap $Width, $Height
$G = [System.Drawing.Graphics]::FromImage($View)
$G.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$G.Clear([System.Drawing.Color]::FromArgb(210,216,222))
$G.FillRectangle((B '#B8B9B3'), 0, 0, $Width, 500)
$Floor = [System.Drawing.Point[]]@(
    [System.Drawing.Point]::new(0,500),
    [System.Drawing.Point]::new($Width,500),
    [System.Drawing.Point]::new($Width,1080),
    [System.Drawing.Point]::new(0,1080)
)
$G.FillPolygon((B '#8A8C85'), $Floor)
$G.FillRectangle((B '#C9CAC4'), 0, 0, $Width, 90)
$G.DrawString('MeetingRoom_Blockout_v3 - Expanded Closed Room / Spawn Inside', $TitleFont, [System.Drawing.Brushes]::Black, 90, 38)
$G.FillRectangle((B '#143D94'), 700, 135, 520, 235); $G.DrawRectangle($Pen, 700, 135, 520, 235); DrawLabel $G 'screen_01' 960 252 $Font ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#EBEFE6'), 145, 210, 320, 190); $G.DrawRectangle($Pen, 145, 210, 320, 190); DrawLabel $G 'whiteboard_01' 305 305 $Font ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#574C40'), 1390, 390, 245, 345); $G.DrawRectangle($Pen, 1390, 390, 245, 345); DrawLabel $G 'cabinet_01' 1512 560 $Font ([System.Drawing.Brushes]::Black)
$Table = [System.Drawing.Point[]]@(
    [System.Drawing.Point]::new(530,610),
    [System.Drawing.Point]::new(1390,610),
    [System.Drawing.Point]::new(1560,875),
    [System.Drawing.Point]::new(360,875)
)
$G.FillPolygon((B '#6B4221'), $Table); $G.DrawPolygon($Pen, $Table); DrawLabel $G 'meeting_table_01' 960 740 $Font ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#15171A'), 625, 650, 150, 70); $G.DrawRectangle($Pen, 625, 650, 150, 70); DrawLabel $G 'laptop_01' 700 633 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#DB261A'), 1095, 684, 190, 22); $G.DrawRectangle($Pen, 1095, 684, 190, 22); DrawLabel $G 'wrong_cable_01' 1190 665 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#F5EDC7'), 920, 710, 130, 70); $G.DrawRectangle($Pen, 920, 710, 130, 70); DrawLabel $G 'document_01' 985 700 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#26CC52'), 830, 782, 260, 88); $G.DrawRectangle($Pen, 830, 782, 260, 88); DrawLabel $G 'table_target_area' 960 826 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#1266E6'), 1435, 342, 175, 18); $G.DrawRectangle($Pen, 1435, 342, 175, 18); DrawLabel $G 'hdmi_cable_01' 1522 322 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#080809'), 1455, 372, 76, 28); $G.DrawRectangle($Pen, 1455, 372, 76, 28); DrawLabel $G 'remote_01' 1493 420 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#2ECC47'), 885, 980, 150, 38); $G.DrawRectangle($Pen, 885, 980, 150, 38); DrawLabel $G 'start_point' 960 960 $SmallFont ([System.Drawing.Brushes]::Black)
$G.FillRectangle((B '#FAAD29'), 820, 470, 300, 35); $G.DrawRectangle($Pen, 820, 470, 300, 35); DrawLabel $G 'presentation_point' 970 452 $SmallFont ([System.Drawing.Brushes]::Black)
$View.Save((Join-Path $ShotDir 'meeting_room_blockout_first_person.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$G.Dispose()
$View.Dispose()

Write-Output "Generated scene: $ScenePath"
Write-Output "Generated docs: $(Join-Path $DocDir 'Object_ID_List.md')"
Write-Output "Generated screenshots: $ShotDir"
