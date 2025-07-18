# UI Clipping Mask Setup Guide

This guide explains how to set up proper UI clipping using the custom clipping mask image to eliminate the blinking effect and create smooth tape scrolling.

## Overview

The previous masking system used conditional hiding/showing of individual tick labels, which created a blinking effect. This new approach uses Unity's UI Mask component with your custom clipping shape for smooth, professional clipping.

## Required Assets

- **Clipping Mask Image**: `Assets/Images/UIelements/img_clippingmask@2x.png`
- **Alternative**: `Assets/Images/UIelements/img_clippingmask.svg` (if you prefer vector)

## Implementation Steps

### Step 1: Disable Old Masking System

✅ **COMPLETED** - The HUDTapeController script has been updated:
- `enableConditionalMasking = false`
- `enablePositionalMasking = false`
- All tick labels will now be visible (but will be clipped by the mask)

### Step 2: Create Mask Container Hierarchy

In Unity Editor, restructure your Speed Tape hierarchy as follows:

```
SpeedTape (main container)
├── SpeedTapeMask (NEW - with Mask + Image components)
│   └── TickContainer (MOVE under mask)
│       ├── Tick_0
│       ├── Tick_1
│       └── ... (all generated ticks)
└── txt_AirSpeed (current speed display - stays outside mask)
```

### Step 3: Create and Configure SpeedTapeMask GameObject

1. **Create the Mask GameObject**:
   - Right-click on your SpeedTape container
   - Create Empty → Name it "SpeedTapeMask"

2. **Add Required Components**:
   - Select SpeedTapeMask
   - Add Component → UI → Image
   - Add Component → UI → Mask

3. **Configure the Image Component**:
   - Set Source Image to `img_clippingmask@2x`
   - Set Image Type to "Simple"
   - Set Color to White (255, 255, 255, 255)
   - Check "Preserve Aspect" if needed

4. **Configure the Mask Component**:
   - Check "Show Mask Graphic" (temporarily, to see the clipping area)
   - Leave "Invert Mask" unchecked

5. **Position and Scale**:
   - Adjust RectTransform to position the visible area correctly
   - Scale to match your HUD design requirements
   - The white areas of your mask image will be visible, transparent areas will be clipped

### Step 4: Move TickContainer Under Mask

1. **Drag TickContainer**:
   - In the Hierarchy, drag "TickContainer" to be a child of "SpeedTapeMask"
   - This ensures all tick marks and labels are clipped by the mask

2. **Verify Hierarchy**:
   ```
   SpeedTapeMask
   └── TickContainer
       ├── Tick_0 (with Label child)
       ├── Tick_1 (with Label child)
       └── ... (all ticks)
   ```

### Step 5: Fine-tune Mask Settings

1. **Test the Clipping**:
   - Run the game and observe the speed tape
   - Tick marks should smoothly scroll through the clipping area
   - No more blinking labels!

2. **Adjust Mask Position**:
   - Move SpeedTapeMask RectTransform to align with your airspeed indicator
   - Scale to match the desired visible area

3. **Hide Mask Graphic** (Optional):
   - Once positioned correctly, uncheck "Show Mask Graphic" on the Mask component
   - This hides the mask image itself, showing only the clipped content

## Alternative: Using RectMask2D (Better Performance)

If you prefer a simpler rectangular clipping area:

1. **Remove Image and Mask components** from SpeedTapeMask
2. **Add RectMask2D component** instead
3. **Adjust RectTransform size** to define the clipping rectangle
4. **Position** to align with your airspeed indicator

## Benefits of This Approach

### ✅ **Smooth Animation**
- No more blinking labels
- Tick marks slide smoothly through the clipping area
- Professional aircraft HUD appearance

### ✅ **Custom Shape Support**
- Uses your exact clipping mask design
- Supports complex shapes and curves
- Matches your HUD mockup perfectly

### ✅ **Performance Optimized**
- Unity's built-in UI masking is GPU-accelerated
- No script-based hiding/showing of individual elements
- Efficient rendering with proper clipping

### ✅ **Easy to Maintain**
- No complex masking logic in scripts
- Visual setup in Unity Editor
- Easy to adjust mask position and size

## Troubleshooting

### **Mask Not Working**
- Ensure TickContainer is a child of SpeedTapeMask
- Check that Mask component is added to SpeedTapeMask
- Verify Image component has the clipping mask assigned

### **Wrong Clipping Area**
- Adjust SpeedTapeMask RectTransform position and scale
- Check that mask image has proper alpha channel (white = visible, transparent = hidden)
- Enable "Show Mask Graphic" to see the mask boundaries

### **Performance Issues**
- Consider using RectMask2D instead of Mask for simple rectangular clipping
- Reduce the number of visible ticks if needed
- Ensure mask image is properly optimized

## Current Status

✅ **Script Updated**: Conditional masking disabled
⏳ **Manual Setup Required**: Create mask hierarchy in Unity Editor
⏳ **Testing**: Verify smooth clipping behavior

## Next Steps

1. Follow the manual setup steps in Unity Editor
2. Test the smooth scrolling behavior
3. Fine-tune mask position and scale
4. Apply the same approach to altitude tape if needed

This approach will give you the smooth, professional HUD tape animation you're looking for, without any blinking effects!
