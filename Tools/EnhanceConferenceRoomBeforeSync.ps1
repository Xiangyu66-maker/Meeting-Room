$ErrorActionPreference = 'Stop'

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ScenePath = Join-Path $ProjectRoot 'Assets\Scenes\ConferenceRoom_before_blockout_sync.unity'
$MaterialDir = Join-Path $ProjectRoot 'Assets\Materials\Blockout'
$ObjectIdentityMeta = Join-Path $ProjectRoot 'Assets\Scripts\ObjectIdentity.cs.meta'
$BackupDir = Join-Path $PSScriptRoot 'Backups'
$BackupPath = Join-Path $BackupDir 'ConferenceRoom_before_blockout_sync.unity.bak'

$Culture = [System.Globalization.CultureInfo]::InvariantCulture
function F([double]$Value) { $Value.ToString('0.###', $Culture) }
function Vec([double[]]$Value) { "{x: $(F $Value[0]), y: $(F $Value[1]), z: $(F $Value[2])}" }

function Get-MetaGuid([string]$Path) {
    $Line = Select-String -LiteralPath $Path -Pattern '^guid:' | Select-Object -First 1
    if (-not $Line) {
        throw "Missing guid in meta file: $Path"
    }
    $Line.Line.Split(':')[1].Trim()
}

function Set-PrefabOverride([string]$Text, [string]$FileId, [string]$PropertyPath, [string]$Value) {
    $EscapedProperty = [regex]::Escape($PropertyPath)
    $Pattern = "(?ms)(- target: \{fileID: $FileId, guid: c6453f8e1f814744d8b94e5a6d1f9942, type: 3\}\s+propertyPath: $EscapedProperty\s+value: )[^\r\n]+"
    if (-not [regex]::IsMatch($Text, $Pattern)) {
        throw "Could not find prefab override $FileId / $PropertyPath"
    }
    [regex]::Replace($Text, $Pattern, {
        param($Match)
        $Match.Groups[1].Value + $Value
    }, 1)
}

function New-CubeYaml($Object, [string]$ScriptGuid) {
    $Go = $Object.Base
    $Transform = $Go + 1
    $MeshFilter = $Go + 2
    $Collider = $Go + 3
    $Renderer = $Go + 4
    $Identity = $Go + 5
    $ParentTransform = 7100000001
    $Position = Vec $Object.P
    $Scale = Vec $Object.S
    $MaterialGuid = Get-MetaGuid (Join-Path $MaterialDir "Blockout_$($Object.Id).mat.meta")
    $Trigger = if ($Object.Trigger) { 1 } else { 0 }
    @"
--- !u!1 &$Go
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: $Transform}
  - component: {fileID: $MeshFilter}
  - component: {fileID: $Collider}
  - component: {fileID: $Renderer}
  - component: {fileID: $Identity}
  m_Layer: 0
  m_Name: $($Object.Id)
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &$Transform
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: $Position
  m_LocalScale: $Scale
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {fileID: $ParentTransform}
  m_RootOrder: $($Object.Order)
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!33 &$MeshFilter
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: $Go}
  m_Mesh: {fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}
--- !u!65 &$Collider
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
  m_IsTrigger: $Trigger
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Size: {x: 1, y: 1, z: 1}
  m_Center: {x: 0, y: 0, z: 0}
--- !u!23 &$Renderer
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
  - {fileID: 2100000, guid: $MaterialGuid, type: 2}
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
--- !u!114 &$Identity
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

if (-not (Test-Path -LiteralPath $ScenePath)) {
    throw "Scene not found: $ScenePath"
}
if (-not (Test-Path -LiteralPath $BackupPath)) {
    New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null
    Copy-Item -LiteralPath $ScenePath -Destination $BackupPath
}

$ScriptGuid = Get-MetaGuid $ObjectIdentityMeta
$Scene = [System.IO.File]::ReadAllText($ScenePath)

$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalPosition.x' '0'
$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalPosition.y' '0.85'
$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalPosition.z' '-3.8'
$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalRotation.w' '1'
$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalRotation.x' '0'
$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalRotation.y' '0'
$Scene = Set-PrefabOverride $Scene '400002' 'm_LocalRotation.z' '0'

$Objects = @(
    [pscustomobject]@{ Id='room_floor'; Base=7100000010; Order=0; P=[double[]]@(0,-0.05,0); S=[double[]]@(12,0.1,10); Trigger=$false },
    [pscustomobject]@{ Id='wall_front'; Base=7100000020; Order=1; P=[double[]]@(0,1.6,5); S=[double[]]@(12,3.2,0.1); Trigger=$false },
    [pscustomobject]@{ Id='wall_back'; Base=7100000030; Order=2; P=[double[]]@(0,1.6,-5); S=[double[]]@(12,3.2,0.1); Trigger=$false },
    [pscustomobject]@{ Id='wall_left'; Base=7100000040; Order=3; P=[double[]]@(-6,1.6,0); S=[double[]]@(0.1,3.2,10); Trigger=$false },
    [pscustomobject]@{ Id='wall_right'; Base=7100000050; Order=4; P=[double[]]@(6,1.6,0); S=[double[]]@(0.1,3.2,10); Trigger=$false },
    [pscustomobject]@{ Id='ceiling_01'; Base=7100000060; Order=5; P=[double[]]@(0,3.25,0); S=[double[]]@(12,0.1,10); Trigger=$false },
    [pscustomobject]@{ Id='meeting_table_01'; Base=7100000070; Order=6; P=[double[]]@(0,0.4,0); S=[double[]]@(3,0.2,1.5); Trigger=$false },
    [pscustomobject]@{ Id='chair_01'; Base=7100000080; Order=7; P=[double[]]@(-1.2,0.25,-1.35); S=[double[]]@(0.5,0.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='chair_02'; Base=7100000090; Order=8; P=[double[]]@(0,0.25,-1.35); S=[double[]]@(0.5,0.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='chair_03'; Base=7100000100; Order=9; P=[double[]]@(1.2,0.25,-1.35); S=[double[]]@(0.5,0.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='chair_04'; Base=7100000110; Order=10; P=[double[]]@(-1.2,0.25,1.35); S=[double[]]@(0.5,0.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='chair_05'; Base=7100000120; Order=11; P=[double[]]@(0,0.25,1.35); S=[double[]]@(0.5,0.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='chair_06'; Base=7100000130; Order=12; P=[double[]]@(1.2,0.25,1.35); S=[double[]]@(0.5,0.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='cabinet_01'; Base=7100000140; Order=13; P=[double[]]@(5.1,0.75,1.4); S=[double[]]@(1,1.5,0.5); Trigger=$false },
    [pscustomobject]@{ Id='whiteboard_01'; Base=7100000150; Order=14; P=[double[]]@(-5.93,1.45,1.2); S=[double[]]@(0.08,1.1,1.8); Trigger=$false },
    [pscustomobject]@{ Id='screen_01'; Base=7100000160; Order=15; P=[double[]]@(0,1.55,4.93); S=[double[]]@(2.5,1.2,0.08); Trigger=$false },
    [pscustomobject]@{ Id='laptop_01'; Base=7100000170; Order=16; P=[double[]]@(-0.8,0.6,0); S=[double[]]@(0.6,0.05,0.4); Trigger=$false },
    [pscustomobject]@{ Id='hdmi_cable_01'; Base=7100000180; Order=17; P=[double[]]@(5.1,1.55,1.2); S=[double[]]@(0.7,0.04,0.04); Trigger=$false },
    [pscustomobject]@{ Id='wrong_cable_01'; Base=7100000190; Order=18; P=[double[]]@(0.8,0.6,0.3); S=[double[]]@(0.7,0.04,0.04); Trigger=$false },
    [pscustomobject]@{ Id='remote_01'; Base=7100000200; Order=19; P=[double[]]@(4.9,1.6,1.5); S=[double[]]@(0.25,0.04,0.1); Trigger=$false },
    [pscustomobject]@{ Id='document_01'; Base=7100000210; Order=20; P=[double[]]@(0.2,0.62,-0.3); S=[double[]]@(0.4,0.02,0.3); Trigger=$false },
    [pscustomobject]@{ Id='table_target_area'; Base=7100000220; Order=21; P=[double[]]@(0,0.635,0); S=[double[]]@(0.8,0.03,0.5); Trigger=$true },
    [pscustomobject]@{ Id='start_point'; Base=7100000230; Order=22; P=[double[]]@(0,0.035,-3.8); S=[double[]]@(0.65,0.07,0.65); Trigger=$true },
    [pscustomobject]@{ Id='presentation_point'; Base=7100000240; Order=23; P=[double[]]@(0,0.035,3.2); S=[double[]]@(0.65,0.07,0.65); Trigger=$true }
)

if ($Scene -notmatch 'm_Name: BeforeSync_TaskExperimentLayer') {
    $ParentChildren = ($Objects | ForEach-Object { "  - {fileID: $($_.Base + 1)}" }) -join "`r`n"
    $ParentYaml = @"
--- !u!1 &7100000000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 7100000001}
  m_Layer: 0
  m_Name: BeforeSync_TaskExperimentLayer
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &7100000001
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 7100000000}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
$ParentChildren
  m_Father: {fileID: 0}
  m_RootOrder: 6
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
"@

    $ObjectYaml = ($Objects | ForEach-Object { New-CubeYaml $_ $ScriptGuid }) -join "`r`n"
    $InsertYaml = $ParentYaml + "`r`n" + $ObjectYaml + "`r`n"
    $SceneRootsMarker = '--- !u!1660057539 &9223372036854775807'
    $MarkerIndex = $Scene.IndexOf($SceneRootsMarker)
    if ($MarkerIndex -lt 0) {
        throw 'SceneRoots marker not found'
    }
    $Scene = $Scene.Insert($MarkerIndex, $InsertYaml)
    $Scene = $Scene -replace "(\r?\nSceneRoots:\r?\n  m_ObjectHideFlags: 0\r?\n  m_Roots:(?:\r?\n  - \{fileID: [0-9]+\})*)", "`$1`r`n  - {fileID: 7100000001}"
}

[System.IO.File]::WriteAllText($ScenePath, $Scene, [System.Text.UTF8Encoding]::new($false))
Write-Host "Enhanced ConferenceRoom_before_blockout_sync.unity"
