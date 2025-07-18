# UI Masking Setup Guide for HUD Speed Tape

This guide explains how to set up UI masking to prevent tick mark numbers from showing through the airspeed indicator.

## Solution #1: UI Mask Component Setup

### Step 1: Create Mask GameObject
1. In the Unity Hierarchy, find your Speed Tape container
2. Right-click on the Speed Tape container → Create Empty
3. Name it "SpeedTapeMask"
4. Position it to cover the area where the airspeed indicator is located

### Step 2: Add Mask Component
1. Select the "SpeedTapeMask" GameObject
2. In the Inspector, click "Add Component"
3. Search for and add "Mask" component
4. Check "Show Mask Graphic" if you want to see the mask area (optional)

### Step 3: Add Image Component for Mask Shape
1. With "SpeedTapeMask" still selected
2. Add Component → UI → Image
3. Set the Image color to white with full alpha (255, 255, 255, 255)
4. Resize the RectTransform to cover the airspeed indicator area
5. Position it exactly where you want to hide the tick numbers

### Step 4: Move Tick Container Under Mask
1. Drag your tick container (TickContainer) to be a child of "SpeedTapeMask"
2. The tick marks will now be clipped to only show within the mask area

## Solution #2: RectMask2D (Alternative - More Performance)

### Step 1: Add RectMask2D Component
1. Select your Speed Tape container
2. Add Component → UI → Rect Mask 2D
3. This will automatically mask all child UI elements

### Step 2: Adjust Masking Area
1. Resize the RectTransform of the container to define the visible area
2. Only UI elements within this rectangle will be visible

## Current Implementation Status

✅ **Solution #3: Conditional Label Hiding** - IMPLEMENTED
- Added `maskingRange = 2.5f` parameter
- Added `enableConditionalMasking = true` toggle
- Labels within 2.5 units of current speed are automatically hidden
- Prevents overlap with airspeed indicator

## Configuration Options

### In HUDTapeController Inspector:
- **Masking Range**: Distance from current value to hide labels (default: 2.5)
- **Enable Conditional Masking**: Toggle to enable/disable the masking system

### Recommended Settings:
- For Speed Tape: `maskingRange = 2.5f` (hides ±2.5 MPH around current speed)
- For Altitude Tape: `maskingRange = 25f` (hides ±25 feet around current altitude)

## Testing the Implementation

1. Run the game and observe the speed tape
2. Notice that tick numbers near the current speed are hidden
3. As speed changes, the masking zone moves with it
4. Numbers outside the masking range remain visible for reference

## Troubleshooting

**If numbers still show through:**
- Increase the `maskingRange` value
- Ensure `enableConditionalMasking` is checked
- Consider adding UI Mask component for additional clipping

**If too many numbers are hidden:**
- Decrease the `maskingRange` value
- Check that the current speed value is being read correctly

## Benefits of This Approach

1. **Dynamic**: Masking zone follows the current speed automatically
2. **Configurable**: Easy to adjust masking range per tape type
3. **Performance**: No additional UI components needed
4. **Flexible**: Can be combined with UI Mask for additional clipping
