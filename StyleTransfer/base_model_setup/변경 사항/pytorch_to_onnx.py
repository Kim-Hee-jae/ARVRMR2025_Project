import os
import argparse
from pathlib import Path

import torch
import torch.nn as nn
from PIL import Image
from torchvision import transforms
from torchvision.utils import save_image

import net
from function import coral, style_transfer


## Model Class
class AdaIN(nn.Module):
    def __init__(self, vgg, decoder, alpha):
        super(AdaIN, self).__init__()
        self.vgg = vgg
        self.decoder = decoder
        self.alpha = alpha

    def forward(self, content, style, alpha=1.0, interpolation_weights=None, device='cpu'):
        return style_transfer(self.vgg, self.decoder, content, style, self.alpha, interpolation_weights, device)


## Options
parser = argparse.ArgumentParser()
# Path options
parser.add_argument('--vgg_path', type=str, default='models/vgg_normalised.pth')
parser.add_argument('--decoder_path', type=str, default='models/decoder.pth')

parser.add_argument('--output_path', type=str, default='../../onnx_models/AdaIN.onnx')

# Input options
parser.add_argument('--content_size', type=int, default=512,
                    help='New (minimum) size for the content image, \
                    keeping the original size if set to 0')
parser.add_argument('--style_size', type=int, default=512,
                    help='New (minimum) size for the style image, \
                    keeping the original size if set to 0')

# Advanced options
parser.add_argument('--alpha', type=float, default=1.0,
                    help='The weight that controls the degree of \
                             stylization. Should be between 0 and 1')

# parser.add_argument('--preserve_color', action='store_true',
#                     help='If specified, preserve color of the content image')
# parser.add_argument(
#     '--style_interpolation_weights', type=str, default='',
#     help='The weight for blending the style of multiple style images')

## Check Options
args = parser.parse_args()
assert os.path.splitext(args.output_path)[1] == '.onnx'

output_dir = os.path.dirname(args.output_path)
if not os.path.exists(output_dir):
    os.mkdir(output_dir)

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

## Prepare Inputs
d_content = torch.randn(1, 3, args.content_size, args.content_size).to(device)
d_style= torch.randn(1, 3, args.style_size, args.style_size).to(device)

## Prepare Model
decoder = net.decoder
vgg = net.vgg

decoder.eval()
vgg.eval()

decoder.load_state_dict(torch.load(args.decoder_path))
vgg.load_state_dict(torch.load(args.vgg_path))
vgg = nn.Sequential(*list(vgg.children())[:31])

vgg.to(device)
decoder.to(device)

## Export Model
adain = AdaIN(vgg, decoder, args.alpha)
adain.to(device)
adain.eval()
torch.onnx.export(adain, (d_content, d_style),
                  args.output_path, opset_version=None,
                  input_names=['content', 'style'],
                  output_names=['output'])

print(f'Onnx file saved at: {args.output_path}')
