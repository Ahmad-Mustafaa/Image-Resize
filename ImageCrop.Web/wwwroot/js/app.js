// Global variables
let cropper = null;
let currentImageId = null;
let currentImageInfo = null;

// DOM Elements
const uploadArea = document.getElementById('uploadArea');
const fileInput = document.getElementById('fileInput');
const imagePreview = document.getElementById('imagePreview');
const previewImage = document.getElementById('previewImage');
const controlsSection = document.getElementById('controlsSection');
const resultSection = document.getElementById('resultSection');

// Aspect Ratio Buttons
const aspectBtns = document.querySelectorAll('.aspect-btn');
const maintainAspectRatio = document.getElementById('maintainAspectRatio');
const resizeWidth = document.getElementById('resizeWidth');
const resizeHeight = document.getElementById('resizeHeight');
const quality = document.getElementById('quality');
const qualityValue = document.getElementById('qualityValue');
const processBtn = document.getElementById('processBtn');
const resetBtn = document.getElementById('resetBtn');
const downloadBtn = document.getElementById('downloadBtn');

// Upload Area Events
uploadArea.addEventListener('click', () => fileInput.click());
uploadArea.addEventListener('dragover', (e) => {
    e.preventDefault();
    uploadArea.classList.add('drag-over');
});
uploadArea.addEventListener('dragleave', () => {
    uploadArea.classList.remove('drag-over');
});
uploadArea.addEventListener('drop', (e) => {
    e.preventDefault();
    uploadArea.classList.remove('drag-over');
    const file = e.dataTransfer.files[0];
    if (file && file.type.startsWith('image/')) {
        handleFileUpload(file);
    }
});

fileInput.addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (file) {
        handleFileUpload(file);
    }
});

// Handle File Upload
async function handleFileUpload(file) {
    const formData = new FormData();
    formData.append('file', file);

    try {
        showLoading(processBtn, 'Uploading...');

        const response = await fetch('/ImageCrop/Api/Upload', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: formData
        });

        console.log('Upload Response Status:', response.status);
        if (!response.ok) {
            const text = await response.text();
            console.error('Upload Response Error:', text);
            throw new Error(`Server returned ${response.status}: ${text}`);
        }

        const text = await response.text();
        console.log('Upload Response Body:', text);
        const result = text ? JSON.parse(text) : {};

        if (result.success) {
            currentImageId = result.imageInfo.imageId;
            currentImageInfo = result.imageInfo;

            // Display image info
            document.getElementById('infoDimensions').textContent = `${result.imageInfo.width} × ${result.imageInfo.height}`;
            document.getElementById('infoFormat').textContent = result.imageInfo.format.toUpperCase();
            document.getElementById('infoSize').textContent = formatFileSize(result.imageInfo.fileSizeBytes);

            // Load image into cropper
            previewImage.src = result.imageInfo.previewUrl;
            imagePreview.classList.add('active');
            controlsSection.style.display = 'block';

            // Initialize Cropper.js
            if (cropper) {
                cropper.destroy();
            }

            cropper = new Cropper(previewImage, {
                viewMode: 1,
                dragMode: 'move',
                aspectRatio: NaN,
                autoCropArea: 0.8,
                restore: false,
                guides: true,
                center: true,
                highlight: true,
                cropBoxMovable: true,
                cropBoxResizable: true,
                toggleDragModeOnDblclick: false,
                crop: updateCropInfo
            });
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Upload error:', error);
        alert('Error uploading image: ' + error.message);
    } finally {
        hideLoading(processBtn, '✨ Process Image');
    }
}

// Update Crop Info
function updateCropInfo(event) {
    const data = event.detail;
    document.getElementById('cropX').textContent = Math.round(data.x);
    document.getElementById('cropY').textContent = Math.round(data.y);
    document.getElementById('cropWidth').textContent = Math.round(data.width);
    document.getElementById('cropHeight').textContent = Math.round(data.height);
}

// Aspect Ratio Buttons
aspectBtns.forEach(btn => {
    btn.addEventListener('click', () => {
        aspectBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');

        const ratio = btn.dataset.ratio;
        if (cropper) {
            if (ratio === 'free') {
                cropper.setAspectRatio(NaN);
            } else {
                cropper.setAspectRatio(parseFloat(ratio));
            }
        }
    });
});

// Resize Input Handlers
resizeWidth.addEventListener('input', () => {
    if (maintainAspectRatio.checked && currentImageInfo && resizeWidth.value) {
        const aspectRatio = currentImageInfo.width / currentImageInfo.height;
        resizeHeight.value = Math.round(resizeWidth.value / aspectRatio);
    }
});

resizeHeight.addEventListener('input', () => {
    if (maintainAspectRatio.checked && currentImageInfo && resizeHeight.value) {
        const aspectRatio = currentImageInfo.width / currentImageInfo.height;
        resizeWidth.value = Math.round(resizeHeight.value * aspectRatio);
    }
});

// Quality Slider
quality.addEventListener('input', () => {
    qualityValue.textContent = quality.value + '%';
});

// Process Button
processBtn.addEventListener('click', async () => {
    if (!cropper || !currentImageId) {
        alert('Please upload an image first');
        return;
    }

    const cropData = cropper.getData();

    const request = {
        imageId: currentImageId,
        crop: {
            x: Math.round(cropData.x),
            y: Math.round(cropData.y),
            width: Math.round(cropData.width),
            height: Math.round(cropData.height)
        },
        resize: null,
        convert: {
            targetFormat: document.getElementById('outputFormat').value,
            quality: parseInt(quality.value)
        },
        compress: {
            quality: parseInt(quality.value)
        }
    };

    // Add resize if specified
    if (resizeWidth.value || resizeHeight.value) {
        request.resize = {
            width: resizeWidth.value ? parseInt(resizeWidth.value) : null,
            height: resizeHeight.value ? parseInt(resizeHeight.value) : null,
            maintainAspectRatio: maintainAspectRatio.checked
        };
    }

    try {
        showLoading(processBtn, 'Processing...');

        const response = await fetch('/ImageCrop/Api/Process', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(request)
        });

        console.log('Process Response Status:', response.status);
        if (!response.ok) {
            const text = await response.text();
            console.error('Process Response Error:', text);
            throw new Error(`Server returned ${response.status}: ${text}`);
        }

        const text = await response.text();
        console.log('Process Response Body:', text);
        const result = text ? JSON.parse(text) : {};

        if (result.success) {
            displayResults(result.data);
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('Processing error:', error);
        alert('Error processing image: ' + error.message);
    } finally {
        hideLoading(processBtn, '✨ Process Image');
    }
});

// Display Results
function displayResults(data) {
    // Original image
    document.getElementById('originalResult').src = currentImageInfo.previewUrl;
    document.getElementById('originalInfo').innerHTML = `
        <p><strong>Dimensions:</strong> ${currentImageInfo.width} × ${currentImageInfo.height}</p>
        <p><strong>Format:</strong> ${currentImageInfo.format.toUpperCase()}</p>
        <p><strong>Size:</strong> ${formatFileSize(currentImageInfo.fileSizeBytes)}</p>
    `;

    // Processed image
    const mimeType = data.format === 'jpg' ? 'jpeg' : data.format;
    const processedSrc = `data:image/${mimeType};base64,${data.base64Data}`;
    document.getElementById('processedResult').src = processedSrc;
    document.getElementById('processedInfo').innerHTML = `
        <p><strong>Dimensions:</strong> ${data.width} × ${data.height}</p>
        <p><strong>Format:</strong> ${data.format.toUpperCase()}</p>
        <p><strong>Size:</strong> ${formatFileSize(data.fileSizeBytes)}</p>
        <p><strong>Reduction:</strong> ${calculateReduction(currentImageInfo.fileSizeBytes, data.fileSizeBytes)}</p>
    `;

    // Store processed image for download
    downloadBtn.onclick = () => {
        try {
            const blob = base64ToBlob(data.base64Data, `image/${mimeType}`);
            const url = URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `processed-image.${data.format}`;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            setTimeout(() => URL.revokeObjectURL(url), 100);
        } catch (e) {
            console.error('Download error:', e);
            alert('Error downloading image. Please try again.');
        }
    };

    resultSection.classList.add('active');
    resultSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// Reset Button
resetBtn.addEventListener('click', () => {
    if (confirm('Are you sure you want to reset? This will clear all your work.')) {
        location.reload();
    }
});

// Utility Functions
function formatFileSize(bytes) {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
}

function calculateReduction(original, processed) {
    const reduction = ((original - processed) / original * 100).toFixed(1);
    const color = reduction > 0 ? '#48bb78' : '#f56565';
    return `<span style="color: ${color}">${reduction > 0 ? '-' : '+'}${Math.abs(reduction)}%</span>`;
}

function showLoading(button, text) {
    button.disabled = true;
    button.innerHTML = `<span class="spinner"></span><span>${text}</span>`;
}


function hideLoading(button, text) {
    button.disabled = false;
    button.innerHTML = `<span>${text}</span>`;
}

function base64ToBlob(base64, mimeType) {
    const byteCharacters = atob(base64);
    const byteArrays = [];

    for (let offset = 0; offset < byteCharacters.length; offset += 512) {
        const slice = byteCharacters.slice(offset, offset + 512);
        const byteNumbers = new Array(slice.length);
        for (let i = 0; i < slice.length; i++) {
            byteNumbers[i] = slice.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        byteArrays.push(byteArray);
    }

    return new Blob(byteArrays, { type: mimeType });
}
