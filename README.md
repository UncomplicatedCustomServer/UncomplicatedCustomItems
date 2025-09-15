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
- **In-Game Commands**: Manage your custom items directly from the server console
- **Active Community**: Join our Discord server for support and exclusive previews
- **Special Perks**: Verified server owners get access to special privileges and early features

## Requirements

- **LabApi** >= `v1.1.0`
- **SCP: Secret Laboratory** server

## Installation

1. Download the latest `UncomplicatedCustomItems.dll` from our [releases page](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomItems/releases/latest)
2. Place the file in your server's `LabApi/Port/Plugins` directory
3. Restart your server
4. Configure your custom items in the generated configuration files

## Quick Start

After installation, UCI will generate default configuration files in your server's config directory. Here's a basic example of creating a custom item:

```yaml
# Sets the ID of the custom item. Custom items cannot share IDs.
id: 2
# Sets the name of the custom item.
name: FunnyGun
# Sets the description of the custom item. This is shown as part of a hint when the item is equipped or picked up.
description: A weapon that has a shotgun-like bullet spread
# The extended description of the custom item. Used by the `.customiteminfo` command
extended_description: ''
# Sets the badge name of the custom item. Remove the text in quotes to disable it.
badge_name: FunnyGun
# Sets the badge color of the custom item.
badge_color: pumpkin
# Sets the weight of the custom item. This affects movement speed when equipped.
weight: 2
# Sets the item the custom item will use.
item: GunFRMG0
# Sets the scale of the custom item when dropped.
scale:
  x: 1
  y: 1
  z: 1
# Defines the spawn settings for the custom item. Information on rooms can be found in the UCI Information forum on Discord.
spawn:
# If true, the custom item can spawn. If false, it will not.
  do_spawn: true
  # The number of custom items to spawn.
  count: 1
  spawn_settings:
  - chance: 30
    rotation:
      x: 0
      y: 0
      z: 0
      w: 0
    # The room(s) where the custom item can spawn.
    dynamic_spawn:
    - room: Lcz914
      coords:
        x: 1
        y: 1
        z: 1
# Sets the custom flags of the custom item. Information about custom flags can be found in the UCI Information forum on Discord.
custom_flags: InfiniteAmmo, ItemGlow, DistruptorTracer
# Settings for the CustomFlags. You can remove any unused settings.
flag_settings: []
arguments:
  OnShotWeapon: action Example
  OnAimedWeapon: Player::Damage(10, "Test", 'AIMING')
# Sets the custom data type the item will use.
custom_item_type: Weapon
# Specifies the modifications the custom item will have.
custom_data:
  damage: 2.75
  max_ammo: 150
  max_magazine_ammo: 150
  max_barrel_ammo: 100
  penetration: 1.24000001
  inaccuracy: 1.24000001
  aiming_inaccuracy: 1.24000001
  damage_falloff_distance: 1
  attachments: DotScope
  enable_friendly_fire: false
```

For detailed configuration options and examples, check our [documentation](#documentation).

## Documentation

Visit the **[UCS Wiki](https://docs.uci.ucsserver.it)** for comprehensive guides, examples, and API documentation.

## Support

### Encountering Issues?

1. **Generate Logs**: Run `ucilogs` in your server console
2. **Report the Issue**: Post the logs in our [Bug Report Forum](https://discord.com/channels/1170301876990914631/1230615155193151602)
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
  - Email: `baguetter@thaumielscpsl.site`

### Community

- **Discord Server**: [https://discord.gg/5StRGu8EJV](https://discord.gg/5StRGu8EJV)
- **GitHub**: [UncomplicatedCustomServer](https://github.com/UncomplicatedCustomServer)

---

<div align="center">
  <p><strong>Made with ❤️ by the UncomplicatedCustomServer Collective</strong></p>
</div>