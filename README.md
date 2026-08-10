<div align="center">
  <img src="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/blob/074974d4c5f5455b15b425ba184b61e972873719/test_promo_banner.png" alt="UncomplicatedCustomItems Banner">
  
  <p>YAML based CustomItem framework for SCP: Secret Laboratory.</p>
  
  <p>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest">
      <img src="https://img.shields.io/github/v/release/UncomplicatedCustomServer/UncomplicatedCustomItems?style=flat-square" alt="Latest Release">
    </a>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest">
      <img src="https://img.shields.io/github/downloads/UncomplicatedCustomServer/UncomplicatedCustomItems/total?style=flat-square" alt="Total Downloads">
    </a>
    <a href="https://discord.gg/5StRGu8EJV">
      <img src="https://img.shields.io/discord/1170301876990914631?color=7289da&logo=discord&logoColor=white&style=flat-square" alt="Discord">
    </a>
  </p>
</div>

---

## Overview

**UncomplicatedCustomItems (UCI)** is a plugin for SCP: Secret Laboratory that lets server owners define and configure custom items using dedicated YAML configuration files. It works natively on both LabAPI and Exiled plugin frameworks.

## Features

- **YAML-Based Configuration**: Define item stats, scales, descriptions, and custom behaviors outside of standard plugin configs.
- **Custom Modules & Effects**: Add item glowing, disruptor tracers, infinite ammo, hint overrides, and custom event triggers.
- **Locker & World Spawns**: Configure drop chances, room locations, zones, and specific locker spawns per item.
- **Dual Framework Support**: Works across both LabAPI and Exiled installations without feature loss.
- **In-Game Administration**: Manage and spawn items directly via server console or Remote Admin commands.

## Requirements

| Framework | Minimum Version |
|-----------|-----------------|
| **LabApi** | `>= v1.1.7`    |
| **Exiled** | `>= v9.9.2`    |

## Installation

### LabApi
1. Download `UncomplicatedCustomItems-LabApi.dll` from the [releases page](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest).
2. Drop the file into your server's `LabApi/Port/Plugins` directory.
3. Restart the server to generate default configuration files.

### Exiled
1. Download `UncomplicatedCustomItems-Exiled.dll` from the [releases page](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest).
2. Drop the file into your server's `Exiled/Plugins` directory.
3. Restart the server to generate default configuration files.

---

## Configuration Example

Below is a basic custom weapon file generated in the configs directory:

```yaml
id: 2
name: FunnyGun
description: A weapon that has a shotgun-like bullet spread
extended_description: ''
badge_name: FunnyGun
badge_color: pumpkin
weight: 2
item: GunFRMG0
scale:
  x: 1
  y: 1
  z: 1
spawn:
  do_spawn: true
  count: 1
  spawn_settings:
  - chance: 100
    locker_settings:
      enable: true
      locker_type: RifleRack
      room: HczWarhead
      zone: HeavyContainment
      chamber: MainChamber
      offset:
        x: 0
        y: 0
        z: 0
  - chance: 100
    locker_settings:
      enable: true
      locker_type: RifleRack
      room: Hcz049
      zone: HeavyContainment
      chamber: MainChamber
      offset:
        x: 0
        y: 0
        z: 0
custom_modules:
  InfiniteAmmo: []
  ItemGlow:
  - GlowColor: '#00FF00'
    range: 5
    intensity: 2
  DistruptorTracer: []
  PickupHintOverride:
  - hint: 'This is a funny gun!'
    duration: 5
  EquipHintOverride:
  - hint: 'You have equipped the funny gun!'
    duration: 5
arguments:
  OnShotWeapon: action Example
  OnAimedWeapon: Player::Damage(10, "Test", 'AIMING')
custom_item_type: Weapon
custom_data:
  damage: 2.75
  max_ammo: 150
  max_magazine_ammo: 150
  max_barrel_ammo: 15
  penetration: 1.24000001
  inaccuracy: 1.24000001
  aiming_inaccuracy: 1.24000001
  damage_falloff_distance: 1
  attachments: DotSight
  enable_friendly_fire: false
```

Detailed guides and property descriptions are available on the [UCI Documentation Wiki](https://docs.uci.ucsserver.it).

---

## Support & Bug Reporting

If you run into bugs or issues:

1. Run `ucilogs` in your server console to generate a log package.
2. Open a ticket in the `#bug-reports` forum on our [Discord Server](https://discord.gg/5StRGu8EJV).
3. Include the generated Log ID and steps to reproduce the issue.

---

## Maintainers

- **FoxWorn3365** (`@foxworn`) — `foxworn3365@gmail.com`
- **Dr.Agenda** (`@dr.agenda`)
- **Mr. Baguetter** (`@ender1992`) — `Mr.Baguetter1@gmail.com`

---

<div align="center">
  <p>Maintained by the <strong>UncomplicatedCustomServer</strong> project.</p>
</div>