<div align="center">
  <img src="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/blob/074974d4c5f5455b15b425ba184b61e972873719/test_promo_banner.png" alt="UncomplicatedCustomItems Banner">
  
  <p><em>Easy, fully configurable and customizable custom items for your SCP:SL Server!</em></p>
  
  <p>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest">
      <img src="https://img.shields.io/github/v/release/UncomplicatedCustomServer/UncomplicatedCustomItems" alt="Latest Release">
    </a>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest">
      <img src="https://img.shields.io/github/downloads/UncomplicatedCustomServer/UncomplicatedCustomItems/total" alt="Total Downloads">
    </a>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/pulls">
      <img src="https://img.shields.io/github/issues-pr/UncomplicatedCustomServer/UncomplicatedCustomItems" alt="Open PRs">
    </a>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/pulls">
      <img src="https://img.shields.io/github/issues-pr-closed/UncomplicatedCustomServer/UncomplicatedCustomItems" alt="Closed PRs">
    </a>
    <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/commits/main/">
      <img src="https://badgen.net/github/commits/UncomplicatedCustomServer/UncomplicatedCustomItems/main" alt="Commits">
    </a>
  </p>
  
  <p>
    <a href='https://discord.gg/5StRGu8EJV'>
      <img src='https://img.shields.io/discord/1170301876990914631?color=7289da&logo=discord&logoColor=white' alt="Discord Server" height="30">
    </a>
  </p>
</div>

---

## Table of Contents
- [About](#about)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Documentation](#documentation)
- [Support](#support)
- [Contributing](#contributing)
- [Donate](#donate)
- [Contact](#contact)

## About

**UncomplicatedCustomItems** (UCI) is a powerful LabAPI and Exiled plugin that enables server administrators to create fully customizable items for SCP: Secret Laboratory servers using simple YAML configuration files. Whether you want to modify existing items or create entirely new ones, UCI provides an intuitive and flexible solution.

## Features

- **Unlimited Custom Items**: Create as many custom items as your server needs
- **YAML Configuration**: Easy-to-use configuration files separate from LabApi's default config
- **Comprehensive Customization**: Configure item properties, damage values, descriptions, and behaviors
- **In-Game Commands**: Manage your custom items directly from the server console or RA menu
- **Active Community**: Join our Discord server for support and exclusive previews
- **Special Perks**: Verified server owners get access to special privileges and early features

## Requirements

- **LabApi** >= `v1.1.5` **or** **Exiled** >= `v9.9.2`

## Installation 
#### LabApi:
1. Download the `UncomplicatedCustomItems-LabApi.dll` from our [releases page](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest)
2. Place the file in your server's `LabApi/Port/Plugins` directory
3. Restart your server
4. Configure your custom items in the generated configuration files

#### Exiled:
1. Download the `UncomplicatedCustomItems-Exiled.dll` from our [releases page](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest)
2. Place the file in your server's `Exiled/Plugins` directory
3. Restart your server
4. Configure your custom items in the generated configuration files


## Quick Start

After installation, UCI will generate default configuration files in your server's config directory. Here's a basic example of creating a custom item:

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

For detailed configuration options and examples, check our [documentation](#documentation).

## Documentation

Visit the **[UCI Wiki](https://docs.uci.ucsserver.it)** for comprehensive guides, examples, and API documentation.

## Support

### Encountering Issues?

1. **Generate Logs**: Run `ucilogs` in your server console
2. **Report the Issue**: Post the log ID in our [Bug Report Forum](https://discord.com/channels/1170301876990914631/1230615155193151602)
3. **Include Details**: Please provide steps to reproduce the issue

### Community Help

Join our [Discord server](https://discord.gg/5StRGu8EJV) for:
- General support and troubleshooting
- Feature requests and suggestions
- Community discussions
- Early access to new features (verified servers)

## Contributing

We welcome contributions! Please feel free to:
- Submit bug reports and feature requests
- Create pull requests for improvements
- Help improve documentation
- Share your custom item configurations with the community

## Donate

All UncomplicatedCustomServer plugins are **free** and **open-source**. If you find our work valuable, please consider supporting us:

<a href="https://opencollective.com/ucs">
  <img src="https://img.shields.io/badge/OpenCollective-Donate-blue?logo=opencollective" alt="Donate on OpenCollective">
</a>

Your donations help us maintain and improve our plugins for the entire SCP:SL community.

## Contact

### Project Team

- **FoxWorn3365**
  - Discord: `@foxworn`
  - Email: `foxworn3365@gmail.com`

- **Dr.Agenda**
  - Discord: `@dr.agenda`

- **Mr. Baguetter**
  - Discord: `@ender1992`
  - Email: `Mr.Baguetter1@gmail.com`

### Community

- **Discord Server**: [https://discord.gg/5StRGu8EJV](https://discord.gg/5StRGu8EJV)

---

<div align="center">
  <p><strong>Made with ❤️ by the UncomplicatedCustomServer Collective</strong></p>
</div>